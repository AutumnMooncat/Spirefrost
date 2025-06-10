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
using static UnityEngine.Rendering.VolumeComponent;

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
                new Sprite[] 
                {
                    MainModFile.instance.wideCompanionFrame,
                    MainModFile.instance.wideCompanionFrameChisel,
                    MainModFile.instance.wideCompanionFrameGold,
                },
                new Sprite[]
                {
                    MainModFile.instance.wideCompanionMask,
                    MainModFile.instance.wideCompanionChiseledMask,
                    MainModFile.instance.wideCompanionChiseledMask
                },
                new Sprite[]
                {
                    MainModFile.instance.wideCompanionOutline,
                    MainModFile.instance.wideCompanionChiseledOutline,
                    MainModFile.instance.wideCompanionChiseledOutline
                },
                new Sprite[]
                {
                    MainModFile.instance.wideCompanionNameTag,
                    MainModFile.instance.wideCompanionChiseledNameTag,
                    MainModFile.instance.wideCompanionChiseledNameTag
                },
                MainModFile.instance.wideCompanionTextBox
                ));
        }

        private static IEnumerator CreateWidePool(string frameName, string toCopy, Sprite[] frameTiers, Sprite[] maskTiers, Sprite[] outlineTiers, Sprite[] nameTagTiers, Sprite textBox)
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
                Sprite mask = maskTiers[i];
                Sprite outline = outlineTiers[i];
                Sprite nameTag = nameTagTiers[i];
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
                // Set result true if valid
                static bool WideCanSpawnCheck(Entity entity, CardSlot desiredSlot, ref bool result)
                {
                    if (entity && entity.data.IsWide())
                    {
                        CardSlot behind = desiredSlot.GetSlotBehind();
                        if (!behind)
                        {
                            return true;
                        }
                        Entity[] blocking = new Entity[] { desiredSlot.GetTop(), behind.GetTop()};
                        if (blocking.All(e => e != null))
                        {
                            return true;
                        }
                        if (blocking.All(e => e == null))
                        {
                            result = true;
                        }
                        else
                        {
                            Entity top = blocking.Where(e => e != null).First();
                            if ((top.positionPriority >= entity.positionPriority && (entity.positionPriority <= 1 || top.positionPriority > entity.positionPriority)) || !Battle.CanPushBack(top))
                            {
                                return true;
                            }
                            CardSlotLane lane = desiredSlot.Group as CardSlotLane;
                            if (!lane)
                            {
                                return true;
                            }
                            if (!lane.slots.Any(slot => slot.GetXCoord() > desiredSlot.GetXCoord() && slot.GetTop() && slot.GetTop().positionPriority >= entity.positionPriority))
                            {
                                result = true;
                            }
                        }
                        return true;
                    }
                    return false;
                }

                // Set result to false if invalid
                static bool WideCanBacklineSpawnCheck(Entity entity, CardSlot desiredSlot, ref bool result)
                {
                    if (entity && entity.data.IsWide())
                    {
                        CardSlot behind = desiredSlot.GetSlotBehind();
                        if (!behind)
                        {
                            result = false;
                            return true;
                        }
                        Entity[] blocking = new Entity[] { desiredSlot.GetTop(), behind.GetTop() };
                        if (blocking.Any(e => e != null && (e.positionPriority > entity.positionPriority) || !Battle.CanPushForwards(e)))
                        {
                            result = false;
                        }
                        return true;
                    }
                    return false;
                }

                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    List<CodeInstruction> codes = instructions.ToList();
                    FieldInfo canPlace = AccessTools.Field(typeof(CardContainer), nameof(CardContainer.canBePlacedOn));
                    FieldInfo slots = AccessTools.Field(typeof(CardSlotLane), nameof(CardSlotLane.slots));
                    MethodInfo getTop = AccessTools.Method(typeof(CardContainer), nameof(CardContainer.GetTop));
                    MemberInfo getItem = AccessTools.Method(typeof(List<CardSlot>), "get_Item");
                    MethodInfo wideCheck = AccessTools.Method(typeof(CanDeployPatch), nameof(WideCanSpawnCheck));
                    MethodInfo wideBacklineCheck = AccessTools.Method(typeof(CanDeployPatch), nameof(WideCanBacklineSpawnCheck));
                    bool callInserted = false;
                    bool backlineCallInserted = false;
                    for (int i = 0; i < codes.Count; i++)
                    {
                        if (!callInserted && codes[i].opcode == OpCodes.Ldloc_S && i + 1 < codes.Count && i - 2 >= 0)
                        {
                            if (codes[i + 1].opcode == OpCodes.Callvirt && codes[i + 1].operand as MethodInfo == getTop && codes[i - 1].opcode == OpCodes.Brfalse && codes[i - 2].opcode == OpCodes.Ldfld && codes[i - 2].operand as FieldInfo == canPlace)
                            {
                                Debug.Log($"WideCardBoardLogic.BattleLogic.CanDeployPatch match found, inserting wide check");
                                callInserted = true;
                                // needs entity, slot, ref flag2
                                yield return new CodeInstruction(OpCodes.Ldarg_1);
                                yield return new CodeInstruction(OpCodes.Ldloc, codes[i].operand);
                                yield return new CodeInstruction(OpCodes.Ldloca, 6);
                                yield return new CodeInstruction(OpCodes.Call, wideCheck);
                                yield return new CodeInstruction(OpCodes.Brtrue, codes[i - 1].operand);
                            }
                        }
                        if (!backlineCallInserted && codes[i].opcode == OpCodes.Ldloc_S && i + 1 < codes.Count && i - 2 >= 0)
                        {
                            if (codes[i + 1].opcode == OpCodes.Ldfld && codes[i + 1].operand as FieldInfo == slots && codes[i - 1].opcode == OpCodes.Brfalse && codes[i - 2].opcode == OpCodes.Ldloc_S)
                            {
                                Debug.Log($"WideCardBoardLogic.BattleLogic.CanDeployPatch match found, inserting backline wide check");
                                backlineCallInserted = true;
                                // needs entity, slot, ref flag4
                                yield return new CodeInstruction(OpCodes.Ldarg_1);
                                yield return new CodeInstruction(OpCodes.Ldloc_S, 18); // cardSlotLane2
                                yield return new CodeInstruction(OpCodes.Ldfld, slots);
                                yield return new CodeInstruction(OpCodes.Ldloc_S, 15); // l
                                yield return new CodeInstruction(OpCodes.Callvirt, getItem);
                                yield return new CodeInstruction(OpCodes.Ldloca, 16); // flag4
                                yield return new CodeInstruction(OpCodes.Call, wideBacklineCheck);
                                yield return new CodeInstruction(OpCodes.Brtrue, codes[i - 1].operand);
                            }
                        }
                        yield return codes[i];
                    }
                }
            }

            [HarmonyPatch(typeof(Battle), nameof(Battle.CanPushBack))]
            internal static class CanPushBackPatch
            {
                // Set result to false if invalid
                static bool WidePushCheck(Entity entity, CardSlot slot, ref bool result)
                {
                    if (entity && entity.data.IsWide())
                    {
                        CardSlot behind = slot.GetSlotBehind();
                        if (!behind)
                        {
                            result = false;
                            return true;
                        }
                        Entity[] blocking = new Entity[] { slot.GetTop(), behind.GetTop() };
                        if (blocking.Any(e => e != null && !Battle.CanPushBack(e)))
                        {
                            result = false;
                        }
                        return true;
                    }
                    return false;
                }

                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    List<CodeInstruction> codes = instructions.ToList();
                    MethodInfo getTop = AccessTools.Method(typeof(CardContainer), nameof(CardContainer.GetTop));
                    MethodInfo wideCheck = AccessTools.Method(typeof(CanPushBackPatch), nameof(WidePushCheck));
                    bool callInserted = false;
                    for (int i = 0; i < codes.Count; i++)
                    {
                        if (!callInserted && codes[i].opcode == OpCodes.Ldloc_S && codes[i].operand is byte b && b == 5 && i - 1 >= 0 && i + 1 < codes.Count)
                        {
                            if (codes[i + 1].opcode == OpCodes.Callvirt && codes[i + 1].operand as MethodInfo == getTop)
                            {
                                Debug.Log($"WideCardBoardLogic.BattleLogic.CanPushBackPatch match found, inserting wide check and moving labels");
                                callInserted = true;
                                CodeInstruction nop = new CodeInstruction(OpCodes.Nop);
                                nop.labels.AddRange(codes[i].labels);
                                codes[i].labels.Clear();
                                yield return nop;
                                // needs entity, slot, ref result
                                yield return new CodeInstruction(OpCodes.Ldloc_S, 5); // cardSlot
                                yield return new CodeInstruction(OpCodes.Ldloca_S, 0); // result
                                yield return new CodeInstruction(OpCodes.Call, wideCheck);
                                yield return new CodeInstruction(OpCodes.Brtrue, codes[i - 1].operand);
                            }
                        }
                        yield return codes[i];
                    }
                }
            }

            [HarmonyPatch(typeof(Battle), nameof(Battle.CanPushForwards))]
            internal static class CanPushForwardsPatch
            {
                static bool Prefix(Entity entity, ref bool __result)
                {
                    if (entity && entity.data.IsWide())
                    {
                        __result = true;
                        CardContainer[] containers = entity.containers;
                        for (int i = 0; i < containers.Length; i++)
                        {
                            if (containers[i] is CardSlotLane cardSlotLane)
                            {
                                CardSlot current = cardSlotLane.slots[cardSlotLane.IndexOf(entity)];
                                CardSlot inFront = current.GetSlotInFront();
                                if (inFront == null || !inFront.Empty)
                                {
                                    __result = false;
                                    break;
                                }
                                inFront = inFront.GetSlotInFront();
                                if (inFront == null || !inFront.Empty)
                                {
                                    __result = false;
                                    break;
                                }
                            }
                        }
                        return false;
                    }
                    return true;
                }
            }

            [HarmonyPatch(typeof(Battle), nameof(Battle.GetOppositeRows))]
            internal static class GetOppositeRowsPatch
            {
                static void Prefix(ref CardContainer[] rows)
                {
                    rows = rows.Distinct().ToArray();
                }
            }
        }

        internal static class BattleSaveLogic
        {
            [HarmonyPatch]
            internal static class CreateCardsInRowsPatch
            {
                private static Type createdType;
                private static readonly Dictionary<CardSlot, ulong> nullMap = new Dictionary<CardSlot, ulong>();
                private static Dictionary<ulong, Entity> foundDict;

                static bool NullCheck(CardSlotLane row, int index, Dictionary<ulong, Entity> entities, BattleEntityData data)
                {
                    if (entities[data.cardSaveData.id] == null)
                    {
                        if (foundDict == null)
                        {
                            foundDict = entities;
                        }
                        nullMap[row.slots[index]] = data.cardSaveData.id;
                    }
                    return false;
                }

                static void ResolveNulls()
                {
                    if (foundDict != null)
                    {
                        foreach (var item in nullMap)
                        {
                            item.Key.Add(foundDict[item.Value]);
                        }
                    }
                    foundDict = null;
                    nullMap.Clear();
                }

                static MethodBase TargetMethod()
                {
                    return SpirefrostUtils.FindEnumeratorMethod(AccessTools.DeclaredMethod(typeof(BattleSaveSystem), nameof(BattleSaveSystem.CreateCardsInRows)), ref createdType);
                }

                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
                {
                    List<CodeInstruction> codes = instructions.ToList();
                    Label skipJump = generator.DefineLabel();
                    FieldInfo d = AccessTools.Field(Type.GetType("BattleSaveSystem+<>c__DisplayClass19_2,Assembly-CSharp"), "d");
                    FieldInfo locals2 = AccessTools.Field(Type.GetType("BattleSaveSystem+<>c__DisplayClass19_2,Assembly-CSharp"), "CS$<>8__locals2");
                    FieldInfo locals1 = AccessTools.Field(Type.GetType("BattleSaveSystem+<>c__DisplayClass19_1,Assembly-CSharp"), "CS$<>8__locals1");
                    FieldInfo row = AccessTools.Field(Type.GetType("BattleSaveSystem+<>c__DisplayClass19_1,Assembly-CSharp"), "row");
                    FieldInfo entities = AccessTools.Field(Type.GetType("BattleSaveSystem+<>c__DisplayClass19_0,Assembly-CSharp"), "entities");
                    FieldInfo slots = AccessTools.Field(typeof(CardSlotLane), nameof(CardSlotLane.slots));
                    MethodInfo nullCheck = AccessTools.Method(typeof(CreateCardsInRowsPatch), nameof(NullCheck));
                    MethodInfo resolveNulls = AccessTools.Method(typeof(CreateCardsInRowsPatch), nameof(ResolveNulls));
                    MethodInfo add = AccessTools.Method(typeof(CardContainer), nameof(CardContainer.Add));bool checkInserted = false;
                    bool jumpInserted = false;
                    bool resolveInserted = false;
                    for (int i = 0; i < codes.Count; i++)
                    {
                        if (!checkInserted && codes[i].opcode == OpCodes.Ldloc_S && i - 2 >= 0 && codes[i - 2].opcode == OpCodes.Callvirt)
                        {
                            if (codes[i - 2].operand is MethodInfo clumpInfo && clumpInfo.DeclaringType == typeof(Routine.Clump) && clumpInfo.Name == "Add")
                            {
                                Debug.Log($"WideCardBoardLogic.BattleSaveLogic.CreateCardsInRowPatch match found, inserting null check and moving labels");
                                checkInserted = true;
                                // move label back
                                CodeInstruction first = new CodeInstruction(OpCodes.Nop);
                                first.labels.AddRange(codes[i].labels);
                                codes[i].labels.Clear();
                                yield return first;
                                // we need slot (or lane and index), the dictionary, and id (or BattleEntityData)
                                yield return new CodeInstruction(OpCodes.Ldloc, 4); // BattleSaveSystem+<>c__DisplayClass19_2
                                yield return new CodeInstruction(OpCodes.Ldfld, locals2); // BattleSaveSystem+<>c__DisplayClass19_1
                                yield return new CodeInstruction(OpCodes.Ldfld, row); // CardSlotLane
                                yield return new CodeInstruction(OpCodes.Ldloc_3); // int i
                                yield return new CodeInstruction(OpCodes.Ldloc, 4);
                                yield return new CodeInstruction(OpCodes.Ldfld, locals2);
                                yield return new CodeInstruction(OpCodes.Ldfld, locals1);
                                yield return new CodeInstruction(OpCodes.Ldfld, entities); // Dict
                                yield return new CodeInstruction(OpCodes.Ldloc, 4);
                                yield return new CodeInstruction(OpCodes.Ldfld, d); // BattleEntityData
                                yield return new CodeInstruction(OpCodes.Call, nullCheck);
                                yield return new CodeInstruction(OpCodes.Brtrue, skipJump);
                            }
                        }

                        if (!jumpInserted && checkInserted && codes[i].opcode == OpCodes.Callvirt && codes[i].operand as MethodInfo == add && i + 1 < codes.Count)
                        {
                            Debug.Log($"WideCardBoardLogic.BattleSaveLogic.CreateCardsInRowPatch match found, inserting jump");
                            jumpInserted = true;
                            codes[i + 1].labels.Add(skipJump);
                        }

                        if (!resolveInserted && codes[i].opcode == OpCodes.Ldarg_0 && i + 2 < codes.Count && codes[i + 1].opcode == OpCodes.Ldfld && codes[i + 2].opcode == OpCodes.Ldfld && codes[i + 2].operand as FieldInfo == entities)
                        {
                            Debug.Log($"WideCardBoardLogic.BattleSaveLogic.CreateCardsInRowPatch match found, inserting null resolver");
                            resolveInserted = true;
                            yield return new CodeInstruction(OpCodes.Call, resolveNulls);
                        }

                        yield return codes[i];
                    }
                }
            }
        }

        internal static class CardHandLogic
        {
            [HarmonyPatch(typeof(CardHand), nameof(CardHand.GetAngle))]
            internal static class HandAnglePatch
            {
                internal static void Postfix(CardHand __instance, int childIndex, ref float __result)
                {
                    float angleAdd = __instance.GetAngleAdd();
                    for (int i = 0; i < __instance.Count; i++)
                    {
                        if (__instance[i].data.IsWide() || (i - 1 >= 0 && __instance[i - 1].data.IsWide()))
                        {
                            if (i <= childIndex)
                            {
                                __result += angleAdd;
                            }
                            __result -= angleAdd / 2f;
                        }
                    }
                }
            }
        }

        internal static class CardContainerLogic
        {
            [HarmonyPatch(typeof(CardContainer), nameof(CardContainer.Add))]
            internal static class AddPatch
            {
                static bool Prefix(CardContainer __instance, Entity entity)
                {
                    if (entity == null)
                    {
                        Debug.LogError($"CardContainer Attempting to add null entity!");
                        return false;
                    }
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
        internal static class CardContainerGridLogic
        {
            [HarmonyPatch(typeof(CardContainerGrid), nameof(CardContainerGrid.GetChildPosition))]
            internal static class GetChildPositionPatch
            {
                static void Logic(CardContainerGrid __instance, Entity child)
                {
                    // To fix:
                    // Wide cards need to be positioned between 2 columns
                    // Wide cards need to occupy 2 slots for pushing other cards 
                    // Wide cards on new row if it already has 4 cards
                    int index = __instance.IndexOf(child);
                    // index +1 for each previous card that was wide
                    // index +1 for each previous wide card shoved to new row
                    int colIndex = index % __instance.columnCount;
                    int rowIndex = Mathf.FloorToInt(index / __instance.columnCount);
                    // If entity is wide and in last column, colIndex = 0, rowIndex += 1
                    int rowCount = __instance.RowCount(rowIndex);
                    float startX = (float)rowCount * __instance.cellSize.x + (float)(rowCount - 1) * __instance.spacing.x;
                    Vector2 sizeDelta = __instance.rectTransform.sizeDelta;
                    Vector2 vector = new Vector2(0f - sizeDelta.x, sizeDelta.y) * 0.5f;
                    switch (__instance.align)
                    {
                        case TextAlignment.Center:
                            vector.x = (0f - startX) * 0.5f;
                            break;
                        case TextAlignment.Right:
                            vector.x = sizeDelta.x * 0.5f - startX;
                            break;
                    }

                    vector.x += __instance.cellSize.x * 0.5f + __instance.spacing.x;
                    vector.y -= __instance.cellSize.y * 0.5f + __instance.spacing.y;
                    Vector2 vector2 = vector;
                    // If entity is wide, colIndex += 0.5 (have to make a float to use)
                    vector2.x += (float)colIndex * __instance.cellSize.x + (float)(colIndex - 1) * __instance.spacing.x;
                    vector2.y -= (float)rowIndex * __instance.cellSize.y + (float)(rowIndex - 1) * __instance.spacing.y;
                    //return (Vector3)vector2 + Vector3.Scale(child.random3, __instance.randomOffset);
                }

                static int GetEffectiveIndex(int index, CardContainerGrid __instance)
                {
                    int wideSpacing = 0;
                    int currColumnIndex = 0;
                    for (int i = 0; i < index; i++)
                    {
                        if (__instance[i].data.IsWide())
                        {
                            wideSpacing++;
                            currColumnIndex++;
                            if (currColumnIndex >= __instance.columnCount)
                            {
                                wideSpacing++;
                                currColumnIndex = 1;
                            }
                        }

                        currColumnIndex++;
                        currColumnIndex %= __instance.columnCount;
                    }
                    return index + wideSpacing;
                }

                static void WideCardToNewRowCheck(CardContainerGrid __instance, Entity entity, ref int colIndex, ref int rowIndex)
                {
                    if (entity.data.IsWide() && colIndex == __instance.columnCount - 1)
                    {
                        colIndex = 0;
                        rowIndex += 1;
                    }
                }

                static float AdjustWideCard(Entity entity)
                {
                    return entity.data.IsWide() ? 0.5f : 0f;
                }

                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
                {
                    List<CodeInstruction> codes = instructions.ToList();
                    MethodInfo indexOf = AccessTools.Method(typeof(CardContainer), nameof(CardContainer.IndexOf));
                    MethodInfo effectiveIndex = AccessTools.Method(typeof(GetChildPositionPatch), nameof(GetEffectiveIndex));
                    MethodInfo newRowCheck = AccessTools.Method(typeof(GetChildPositionPatch), nameof(WideCardToNewRowCheck));
                    MethodInfo adjustWide = AccessTools.Method(typeof(GetChildPositionPatch), nameof(AdjustWideCard));
                    bool newRowCheckInserted = false;
                    for (int i = 0; i < codes.Count; i++)
                    {
                        yield return codes[i];
                        if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand as MethodInfo == indexOf)
                        {
                            // index on stack, need instance
                            Debug.Log($"GetChildPositionPatch - Match found, adjusting index");
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, effectiveIndex);
                        }
                        if (!newRowCheckInserted && codes[i].opcode == OpCodes.Stloc_1)
                        {
                            Debug.Log($"GetChildPositionPatch - Match found, inserting new row check");
                            newRowCheckInserted = true;
                            yield return new CodeInstruction(OpCodes.Ldarg_0); // instance
                            yield return new CodeInstruction(OpCodes.Ldarg_1); // Entity
                            yield return new CodeInstruction(OpCodes.Ldloca, 0); // colIndex
                            yield return new CodeInstruction(OpCodes.Ldloca, 1); // rowIndex
                            yield return new CodeInstruction(OpCodes.Call, newRowCheck);
                        }
                        if (codes[i].opcode == OpCodes.Conv_R4)
                        {
                            if (i - 1 >= 0 && codes[i - 1].opcode == OpCodes.Ldloc_0)
                            {
                                Debug.Log($"GetChildPositionPatch - Match found, adjusting wide card position");
                                yield return new CodeInstruction(OpCodes.Ldarg_1);
                                yield return new CodeInstruction(OpCodes.Call, adjustWide);
                                yield return new CodeInstruction(OpCodes.Add);
                            }
                            if (i - 3 >= 0 && codes[i - 3].opcode == OpCodes.Ldloc_0)
                            {
                                Debug.Log($"GetChildPositionPatch - Match found, adjusting wide card position");
                                yield return new CodeInstruction(OpCodes.Ldarg_1);
                                yield return new CodeInstruction(OpCodes.Call, adjustWide);
                                yield return new CodeInstruction(OpCodes.Add);
                            }
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(CardContainerGrid), nameof(CardContainerGrid.RowCount))]
            internal static class RowCountPatch
            {
                static void Postfix(CardContainerGrid __instance, int rowIndex, ref int __result)
                {
                    int currColumnCount = 0;
                    int currRowIndex = 0;
                    for (int i = 0; i < __instance.Count; i++)
                    {
                        if (__instance[i].data.IsWide())
                        {
                            currColumnCount += 2;
                            if (currColumnCount > __instance.columnCount)
                            {
                                currColumnCount -= 2;
                                currRowIndex++;
                                if (currRowIndex == rowIndex + 1)
                                {
                                    __result = currColumnCount;
                                    break;
                                }

                                currColumnCount = 2;
                            }
                            else if (currColumnCount == __instance.columnCount)
                            {
                                currRowIndex++;
                                if (currRowIndex == rowIndex + 1)
                                {
                                    __result = currColumnCount;
                                    break;
                                }

                                currColumnCount = 0;
                            }
                        }
                        else
                        {
                            currColumnCount++;
                            if (currColumnCount == __instance.columnCount)
                            {
                                currRowIndex++;
                                if (currRowIndex == rowIndex + 1)
                                {
                                    __result = currColumnCount;
                                    return;
                                }
                                currColumnCount = 0;
                            }
                        }   
                    }
                    __result = currColumnCount;
                }
            }

            [HarmonyPatch(typeof(CardContainerGrid), nameof(CardContainerGrid.GetColumnCount))]
            internal static class GetColumnCountPatch
            {
                static void Postfix(CardContainerGrid __instance, ref int __result)
                {
                    __result = Mathf.Min(__instance.columnCount, __instance.Count + __instance.Where(e => e.data.IsWide()).Count());
                }
            }

            [HarmonyPatch(typeof(CardContainerGrid), nameof(CardContainerGrid.GetRowCount))]
            internal static class GetRowCountPatch
            {
                static void Postfix(CardContainerGrid __instance, ref int __result)
                {
                    int currColumnCount = 0;
                    int currRowCount = __instance.Count > 0 ? 1 : 0;
                    for (int i = 0; i < __instance.Count; i++)
                    {
                        if (__instance[i].data.IsWide())
                        {
                            currColumnCount += 2;
                            if (currColumnCount > __instance.columnCount)
                            {
                                currRowCount++;
                                currColumnCount = 2;
                            }
                        }
                        else
                        {
                            currColumnCount++;
                            if (currColumnCount > __instance.columnCount)
                            {
                                currRowCount++;
                                currColumnCount = 1;
                            }
                        }
                    }
                    __result = currRowCount;
                }
            }
        }
    }
}
