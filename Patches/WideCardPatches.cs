using HarmonyLib;
using Deadpan.Enums.Engine.Components.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Pool;

namespace Spirefrost.Patches
{
    internal static class WideUtils
    {
        internal static string WideKey => Extensions.PrefixGUID("isWide", MainModFile.instance);
        internal static string WideCompanionKey => Extensions.PrefixGUID("WideCompanion", MainModFile.instance);

        internal static bool HasWideFrame(CardData data, out string frameKey)
        {
            frameKey = "";
            if (data.cardType.name == "Friendly")
            {
                frameKey = WideCompanionKey;
            }

            return !frameKey.IsNullOrEmpty();
        }

        internal static void CreateWideCardPools()
        {
            References.instance.StartCoroutine(CreateWidePool(
                WideCompanionKey, 
                "Friendly",
                new Sprite[] {
                    MainModFile.instance.wideCompanionFrame,
                    MainModFile.instance.wideCompanionFrameChisel,
                    MainModFile.instance.wideCompanionFrameGold,
                },
                MainModFile.instance.wideCompanionMask,
                MainModFile.instance.wideCompanionOutline,
                MainModFile.instance.wideCompanionTextBox,
                MainModFile.instance.wideCompanionNameTag
                ));
        }

        private static IEnumerator CreateWidePool(string frameName, string toCopy, Sprite[] frameTiers, Sprite mask, Sprite outline, Sprite textBox, Sprite nameTag)
        {
            Transform t = CardManager.instance.transform;
            CardType baseCardType = MainModFile.instance.TryGet<CardType>(toCopy);
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(baseCardType.prefabRef);
            yield return new WaitUntil(() => handle.IsDone);
            GameObject prefab = handle.Result.InstantiateKeepName();
            prefab.name = "(Card) " + frameName;
            GameObject.DontDestroyOnLoad(prefab);
            
            for (int i = 0; i < frameTiers.Length; i++)
            {
                Sprite frame = frameTiers[i];
                ObjectPool<Card> pool = new ObjectPool<Card>(() =>
                {
                    GameObject obj = GameObject.Instantiate(prefab, CardManager.startPos, Quaternion.identity, t);
                    Card card = obj.GetComponent<Card>();
                    SetWideVisuals(card, frame, mask, outline, textBox, nameTag);
                    return card;
                },
                card =>
                {
                    card.OnGetFromPool();
                    card.entity.OnGetFromPool();
                    card.transform.position = CardManager.startPos;
                    card.transform.localRotation = Quaternion.identity;
                    card.transform.localScale = Vector3.one;
                    card.gameObject.SetActive(value: true);
                },
                card =>
                {
                    card.transform.SetParent(t);
                    card.OnReturnToPool();
                    card.entity.OnReturnToPool();
                    Events.InvokeCardPooled(card);
                    card.gameObject.SetActive(value: false);
                },
                card =>
                {
                    UnityEngine.Object.Destroy(card.gameObject);
                },
                collectionCheck: false, 10, 20);

                CardManager.cardPools[frameName + i.ToString()] = pool;
            }

            prefab.SetActive(false);

            Debug.Log($"Custom Card Frame: {frameName}");
        }

        internal static void SetWideVisuals(Card card, Sprite frame, Sprite mask, Sprite outline, Sprite textBox, Sprite nameTag)
        {
            card.frameImage.sprite = frame;

            float xMultiplier = 2.2f;
            Vector2 scaler = new Vector2(xMultiplier, 1f);
            Vector3 charmOffset = new Vector3(1.946f, 0, 0);

            // Canvas rect
            RectTransform canvasTransform = card.canvas.transform as RectTransform;
            canvasTransform.ScaleOffsets(scaler);

            // Front rect, do not modify
            RectTransform frontTransform = card.canvas.transform.Find("Front") as RectTransform;

            // Mask rect
            RectTransform maskTransform = frontTransform.Find("Mask") as RectTransform;
            maskTransform.ScaleOffsets(scaler);

            // BackgroundContainer rect
            RectTransform backgroundContainerTransform = maskTransform.Find("BackgroundContainer") as RectTransform;
            backgroundContainerTransform.ScaleOffsets(scaler);

            // Frame rect
            RectTransform frameTransform = maskTransform.Find("Frame") as RectTransform;
            frameTransform.ScaleOffsets(scaler);

            // FrameOutline rect
            RectTransform outlineTransform = frontTransform.Find("FrameOutline") as RectTransform;
            outlineTransform.ScaleOffsets(scaler);

            // TextBox rect
            RectTransform descriptionBoxTransform = frontTransform.Find("DescriptionBox") as RectTransform;
            descriptionBoxTransform.ScaleOffsets(scaler);

            // NameTag rect
            RectTransform nameTagTransform = descriptionBoxTransform.Find("NameTag") as RectTransform;
            nameTagTransform.ScaleOffsets(scaler);

            // Desc rect, a little messy
            RectTransform descTransform = descriptionBoxTransform.Find("Desc") as RectTransform;
            descTransform.ScaleOffsets(new Vector2(1.45f, 1f));

            // CharmRope rect
            RectTransform charmRopeTransform = descriptionBoxTransform.Find("CharmRope") as RectTransform;
            charmRopeTransform.TranslateViaSet(charmOffset);

            // Charms rect
            RectTransform charmsTransform = descriptionBoxTransform.Find("Charms") as RectTransform;
            charmsTransform.TranslateViaSet(charmOffset);

            // Images
            Image maskImage = maskTransform.GetComponent<Image>();
            maskImage.sprite = mask;

            Image outlineImage = outlineTransform.GetComponent<Image>();
            outlineImage.sprite = outline;

            Image descriptionBoxImage = descriptionBoxTransform.GetComponent<Image>();
            descriptionBoxImage.sprite = textBox;
            Debug.Log($"Blasting DescBox Loader, surely this wont cause problems later");
            descriptionBoxTransform.GetComponent<AddressableSpriteLoader>().Destroy();

            Image nameTagImage = nameTagTransform.GetComponent<Image>();
            nameTagImage.sprite = nameTag;
        }
        
        internal static bool CanShove(CardSlot[] desiredSlots, Entity shover, out Dictionary<Entity, List<CardSlot>> shoveData)
        {
            shoveData = new Dictionary<Entity, List<CardSlot>>();

            foreach (var slot in desiredSlots)
            {
                Entity shovee = slot.GetTop();

                if (!shovee)
                {
                    continue;
                }

                if (!Events.CheckEntityShove(shovee))
                {
                    return false;
                }
            }

            return CanCompoundShove(shover, desiredSlots, out shoveData);
        }

        internal static List<(CardSlot[] position, double weight)> GetAllValidWeightedPositions(Entity shover, Entity shovee, CardSlot[] restricted)
        {
            bool canIncreaseX = shover.positionPriority >= shovee.positionPriority;
            bool canDecreaseX = shover.positionPriority <= shovee.positionPriority;
            bool canChangeRow = shover.data.canShoveToOtherRow;
            List<(CardSlot[] position, double weight)> positions = new List<(CardSlot[], double)>();
            CardSlot[] currentSlots = shovee.GetContainingSlots();
            if (currentSlots.Length == 0)
            {
                return positions;
            }
            List<(int dx, int dy)> offsets = new List<(int, int)>();
            CardSlot anchor = currentSlots[0];
            int anchorX = anchor.GetXCoord();
            int anchorY = anchor.GetYCoord();
            CardSlotLane anchorRow = anchor.GetContainingLane();
            foreach (var slot in currentSlots)
            {
                if (slot == anchor)
                {
                    continue;
                }
                offsets.Add((slot.GetXCoord() - anchorX, slot.GetYCoord() - anchorY));
            }
            List<CardContainer> rows = Battle.instance.GetRows(shovee.owner);
            foreach (var row in rows)
            {
                if (row is CardSlotLane lane)
                {
                    if (!canChangeRow && lane != anchorRow) {
                        continue;
                    }

                    foreach (var slot in lane.slots)
                    {
                        if (restricted.Contains(slot))
                        {
                            continue;
                        }
                        int x = slot.GetXCoord();
                        int y = rows.IndexOf(row);
                        if ((!canIncreaseX && x > anchorX) || (!canDecreaseX && x < anchorX))
                        {
                            continue;
                        }
                        List<CardSlot> foundSlots = new List<CardSlot>
                        {
                            slot
                        };
                        foreach (var (dx, dy) in offsets)
                        {
                            foundSlots.Add(slot.GetRelativeSlot(dx, dy));
                        }
                        if (foundSlots.Any(s => s == null || restricted.Contains(s)))
                        {
                            continue;
                        }
                        positions.Add((foundSlots.ToArray(), GetWeight((anchorX, anchorY), (x, y))));
                    }
                }
            }
            positions.Sort((a, b) =>
            {
                return a.weight.CompareTo(b.weight);
            });
            return positions;
        }

        private static double GetWeight((int x, int y) currentCoord, (int x, int y) newCoord)
        {
            // Prefer right (-x), then left (+x), then up (-y), then down (+y)
            int dx = newCoord.x - currentCoord.x;
            int dy = newCoord.y - currentCoord.y;
            double bias = 0;
            if (dx > 0)
            {
                bias += 0.001;
            }
            if (dy < 0)
            {
                bias += 0.002;
            }
            else if (dy > 0)
            {
                bias += 0.004;
            }
            return Math.Pow(Math.Abs(dx) + Math.Abs(dy), 2) + bias;
        }

        internal static bool CanCompoundShove(Entity shover, CardSlot[] desiredSlots, out Dictionary<Entity, List<CardSlot>> shoveData)
        {
            shoveData = new Dictionary<Entity, List<CardSlot>>();
            List<(Entity entity, List<(CardSlot[] position, double weight)> positions)> validPositionList = new List<(Entity, List<(CardSlot[] position, double weight)>)>();
            foreach (var item in Battle.GetCardsOnBoard(shover.owner))
            {
                if (item != shover)
                {
                    validPositionList.Add((item, GetAllValidWeightedPositions(shover, item, desiredSlots)));
                }
            }
            if (validPositionList.Any(pair => pair.positions.Count == 0))
            {
                return false;
            }
            double best = -1;
            double current = 0;
            RecursiveShoveCheck(validPositionList, shoveData, new Dictionary<Entity, CardSlot[]>(), 0, ref best, ref current);
            return best != -1;
        }

        internal static bool RecursiveShoveCheck(List<(Entity entity, List<(CardSlot[] position, double weight)> positions)> validPositions, Dictionary<Entity, List<CardSlot>> shoveData, Dictionary<Entity, CardSlot[]> currentData, int depth, ref double best, ref double current)
        {
            foreach (var (position, weight) in validPositions[depth].positions)
            {
                if (best != -1 && current + weight >= best)
                {
                    continue;
                }
                if (currentData.Any(pair => pair.Value.Any(slot => position.Contains(slot))))
                {
                    continue;
                }
                
                currentData[validPositions[depth].entity] = position;
                current += weight;
                if (depth + 1 < validPositions.Count)
                {
                    RecursiveShoveCheck(validPositions, shoveData, currentData, depth + 1, ref best, ref current);
                }
                else
                {
                    best = current;
                    foreach (var item in currentData)
                    {
                        shoveData[item.Key] = item.Value.ToList();
                    }
                }
                current -= weight;
                currentData.Remove(validPositions[depth].entity);
            }
            return best != -1;
        }
    }

    internal class WideCardManagerLogic
    {
        [HarmonyPatch(typeof(CardManager), nameof(CardManager.Get))]
        internal class GetPatch
        {
            static bool Prefix(CardData data, CardController controller, Character owner, bool inPlay, bool isPlayerCard, ref Card __result)
            {
                if (MainModFile.instance.HasLoaded && data.IsWide())
                {
                    if (!WideUtils.HasWideFrame(data, out string frameKey))
                    {
                        return true;
                    }

                    int num = (isPlayerCard ? CardFramesSystem.GetFrameLevel(data.name) : 0);
                    Card card = CardManager.cardPools[$"{frameKey}{num}"].Get();
                    card.frameLevel = num;
                    card.entity.data = data;
                    card.entity.inPlay = inPlay;
                    card.hover.controller = controller;
                    card.entity.owner = owner;
                    Events.InvokeEntityCreated(card.entity);
                    __result = card;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(CardManager), nameof(CardManager.ReturnToPool), new Type[] { typeof(Entity), typeof(Card) })]
        static class ReturnToPoolPatch
        {
            static bool Prefix(Entity entity, Card card, ref bool __result)
            {
                if (GameManager.End || entity.inCardPool || !entity.data.IsWide())
                {
                    return true;
                }

                if (!WideUtils.HasWideFrame(entity.data, out string frameKey))
                {
                    return true;
                }

                if (!entity.returnToPool)
                {
                    UnityEngine.Object.Destroy(entity.gameObject);
                }

                CardManager.cardPools[$"{frameKey}{card.frameLevel}"].Release(card);
                __result = true;
                return false;
            }
        }
    }

    internal class WideCardBoardLogic
    {
        internal static class BattleLogic
        {
            [HarmonyPatch(typeof(Battle), nameof(Battle.CanDeploy))]
            internal static class CanDeployPatch
            {

            }

            [HarmonyPatch(typeof(Battle), nameof(Battle.CanPushBack))]
            internal static class CanPushBackPatch
            {

            }

            [HarmonyPatch(typeof(Battle), nameof(Battle.CanPushForwards))]
            internal static class CanPushForwardsPatch
            {

            }
        }

        internal static class CardContainerLogic
        {
            [HarmonyPatch(typeof(CardContainer), nameof(CardContainer.Add))]
            internal static class AddPatch
            {
                static bool Prefix(CardContainer __instance, Entity entity)
                {
                    if (entity.data.IsWide() && __instance.entities.Contains(entity))
                    {
                        return false;
                    }
                    return true;
                }
            }

            [HarmonyPatch(typeof(CardContainer), nameof(CardContainer.Insert))]
            internal static class InsertPatch
            {
                static bool Prefix(CardContainer __instance, Entity entity)
                {
                    if (entity.data.IsWide() && __instance.entities.Contains(entity))
                    {
                        return false;
                    }
                    return true;
                }
            }
        }

        internal static class CardSlotLogic
        {
            [HarmonyPatch(typeof(CardSlot), nameof(CardSlot.Add))]
            internal static class AddPatch
            {

            }

            [HarmonyPatch(typeof(CardSlot), nameof(CardSlot.Insert))]
            internal static class InsertPatch
            {

            }
        }

        internal static class CardSlotLaneLogic
        {
            [HarmonyPatch(typeof(CardSlotLane), nameof(CardSlotLane.Add))]
            internal static class AddPatch
            {

            }

            [HarmonyPatch(typeof(CardSlotLane), nameof(CardSlotLane.Insert))]
            internal static class InsertPatch
            {

            }

            [HarmonyPatch(typeof(CardSlotLane), nameof(CardSlotLane.PushForwards))]
            internal static class PushForwardsPatch
            {

            }

            [HarmonyPatch(typeof(CardSlotLane), nameof(CardSlotLane.PushBackwards))]
            internal static class PushBackwardsPatch
            {

            }

            [HarmonyPatch(typeof(CardSlotLane), nameof(CardSlotLane.MoveChildrenForward))]
            internal static class MoveChildrenForwardPatch
            {

            }

            [HarmonyPatch(typeof(CardSlotLane), nameof(CardSlotLane.CanPush))]
            internal static class CanPushPatch
            {

            }

            [HarmonyPatch(typeof(CardSlotLane), nameof(CardSlotLane.GetPushData))]
            internal static class GetPushDataPatch
            {

            }

            [HarmonyPatch(typeof(CardSlotLane), nameof(CardSlotLane.Remove))]
            internal static class RemovePatch
            {

            }

            [HarmonyPatch(typeof(CardSlotLane), nameof(CardSlotLane.RemoveAt))]
            internal static class RemoveAtPatch
            {

            }
        }

        internal static class CardControllerLogic
        {
            [HarmonyPatch(typeof(CardControllerBattle), nameof(CardControllerBattle.DragUpdatePosition))]
            internal static class DragUpdatePositionPatch
            {
                static bool WideDragCheck(CardControllerBattle __instance, ref Vector3 vector)
                {
                    Entity entity = __instance.dragging;
                    bool isWide = entity.data.IsWide();
                    bool willMoveWide = false;
                    CardSlot hoverSlot = __instance.hoverSlot;
                    bool hoverEmpty = !hoverSlot || hoverSlot.Count < hoverSlot.max;
                    if (!isWide)
                    {
                        if (!hoverEmpty && ShoveSystem.CanShove(hoverSlot.GetTop(), entity, out var desiredShove))
                        {
                            willMoveWide = desiredShove.Keys.Any(e => e.data.IsWide());
                        }
                    }

                    if (isWide || willMoveWide)
                    {
                        CardSlot behindSlot = hoverSlot?.GetSlotBehind();
                        CardSlot[] desiredSlots = isWide ? new CardSlot[] { hoverSlot, behindSlot } : new CardSlot[] { hoverSlot };
                        foreach (var desired in desiredSlots)
                        {
                            if (!desired || !desired.canBePlacedOn || desired.owner != entity.owner || ShoveSystem.Slot == desired)
                            {
                                return true;
                            }
                        }

                        if (desiredSlots.All(s => s.Count < s.max) || entity.actualContainers.All(cont => desiredSlots.Contains(cont)))
                        {
                            vector = desiredSlots.Select(s => s.transform.position).Aggregate(Vector3.zero, (total, current) => total + current) / desiredSlots.Length - Vector3.Scale(entity.offset.localPosition, entity.transform.localScale);
                        }
                        else
                        {
                            if (WideUtils.CanShove(desiredSlots, entity, out var shoveData))
                            {
                                vector = desiredSlots.Select(s => s.transform.position).Aggregate(Vector3.zero, (total, current) => total + current) / desiredSlots.Length - Vector3.Scale(entity.offset.localPosition, entity.transform.localScale);
                                ShoveSystem.ShowShove(hoverSlot, shoveData);
                            }
                        }
                        return true;
                    }
                    return false;
                }

                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    List<CodeInstruction> codes = instructions.ToList();
                    FieldInfo hoverSlot = AccessTools.Field(typeof(CardController), "hoverSlot");
                    MethodInfo wideCheck = AccessTools.Method(typeof(DragUpdatePositionPatch), "WideDragCheck");
                    bool callInserted = false;
                    for (int i = 0; i < codes.Count; i++)
                    {
                        if (!callInserted && codes[i].opcode == OpCodes.Ldarg_0 && i + 3 < codes.Count)
                        {
                            if (codes[i + 1].opcode == OpCodes.Ldfld && codes[i + 2].opcode == OpCodes.Call && codes[i + 3].opcode == OpCodes.Brfalse)
                            {
                                if (codes[i + 1].operand is FieldInfo fi && fi == hoverSlot)
                                {
                                    Debug.Log("CardControllerLogic.DragUpdatePositionPatch - Match found, injecting new instructions");
                                    callInserted = true;
                                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                                    yield return new CodeInstruction(OpCodes.Ldloca, 0);
                                    yield return new CodeInstruction(OpCodes.Call, wideCheck);
                                    yield return new CodeInstruction(OpCodes.Brtrue, codes[i + 3].operand);
                                }
                            }
                        }
                        yield return codes[i];
                    }
                }
            }

            [HarmonyPatch(typeof(CardControllerBattle), nameof(CardControllerBattle.Release))]
            internal static class ReleasePatch
            {
                static bool WideReleaseCheck(CardControllerBattle __instance)
                {
                    Entity entity = __instance.dragging;
                    bool isWide = entity.data.IsWide();
                    bool willMoveWide = false;
                    CardSlot hoverSlot = __instance.hoverSlot;
                    bool hoverEmpty = !hoverSlot || hoverSlot.Count < hoverSlot.max;
                    if (!isWide)
                    {
                        if (!hoverEmpty && ShoveSystem.CanShove(hoverSlot.GetTop(), entity, out var desiredShove))
                        {
                            willMoveWide = desiredShove.Keys.Any(e => e.data.IsWide());
                        }
                    }
                    if (isWide || willMoveWide)
                    {
                        CardSlot behindSlot = hoverSlot?.GetSlotBehind();
                        CardSlot[] desiredSlots = isWide ? new CardSlot[] { hoverSlot, behindSlot } : new CardSlot[] { hoverSlot };
                        foreach (var desired in desiredSlots)
                        {
                            if (!desired || !desired.canBePlacedOn || desired.owner != entity.owner)
                            {
                                return true;
                            }
                        }
                        if (entity.actualContainers.All(cont => desiredSlots.Contains(cont)))
                        {
                            return true;
                        }

                        if (desiredSlots.All(s => s.Count < s.max))
                        {
                            ActionMove move = new ActionMove(entity, desiredSlots);
                            if (Events.CheckAction(move))
                            {
                                bool freeMove = Battle.IsOnBoard(entity) && desiredSlots.All(s => Battle.IsOnBoard(s.Group));
                                Events.InvokeEntityPlace(entity, desiredSlots, freeMove);
                                ActionQueue.Add(move);
                                ActionQueue.Add(new ActionEndTurn(__instance.owner));
                                if (freeMove)
                                {
                                    __instance.owner.freeAction = true;
                                }

                                __instance.enabled = false;
                            }
                        }
                        else
                        {
                            if (WideUtils.CanShove(desiredSlots, entity, out var shoveData))
                            {
                                ActionMove move = new ActionMove(entity, desiredSlots);
                                if (Events.CheckAction(move))
                                {
                                    bool freeMove = Battle.IsOnBoard(entity) && desiredSlots.All(s => Battle.IsOnBoard(s.Group));
                                    ShoveSystem.Fix = true;
                                    Events.InvokeEntityPlace(entity, desiredSlots, freeMove);
                                    ActionQueue.Add(new ActionShove(shoveData));
                                    ActionQueue.Add(move);
                                    ActionQueue.Add(new ActionEndTurn(__instance.owner));
                                    if (freeMove)
                                    {
                                        __instance.owner.freeAction = true;
                                    }

                                    __instance.enabled = false;
                                }
                            }
                        }
                        return true;
                    }
                    return false;
                }

                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    List<CodeInstruction> codes = instructions.ToList();
                    FieldInfo hoverSlot = AccessTools.Field(typeof(CardController), "hoverSlot");
                    MethodInfo wideCheck = AccessTools.Method(typeof(ReleasePatch), "WideReleaseCheck");
                    bool callInserted = false;
                    for (int i = 0; i < codes.Count; i++)
                    {
                        if (!callInserted && codes[i].opcode == OpCodes.Ldloc_S && i + 4 < codes.Count)
                        {
                            if (codes[i + 1].opcode == OpCodes.Ldc_I4_2 && codes[i + 2].opcode == OpCodes.Bne_Un && codes[i + 3].opcode == OpCodes.Ldarg_0 && codes[i + 4].opcode == OpCodes.Ldfld)
                            {
                                if (codes[i + 4].operand is FieldInfo fi && fi == hoverSlot)
                                {
                                    Debug.Log("CardControllerLogic.ReleasePatch - Match found, injecting new instructions");
                                    callInserted = true;
                                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                                    yield return new CodeInstruction(OpCodes.Call, wideCheck);
                                    yield return new CodeInstruction(OpCodes.Brtrue, codes[i + 2].operand);
                                }
                            }
                        }
                        yield return codes[i];
                    }
                }
            }
        }

        [HarmonyPatch]
        internal static class CardMovePatch
        {
            private static Type createdType;
            public static MethodBase TargetMethod()
            {
                return SpirefrostUtils.FindEnumeratorMethod(AccessTools.DeclaredMethod(typeof(Sequences), nameof(Sequences.CardMove)), ref createdType);
            }
        }
    }

    internal class WideCardInventoryLogic
    {

    }
}
