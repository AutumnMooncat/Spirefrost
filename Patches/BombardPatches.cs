using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spirefrost.Patches
{
    [HarmonyPatch]
    internal class BombardPatches
    {
        internal static bool isAlly;

        internal static StatusEffectBombard current;

        internal static bool initColours;

        internal static Color originalColour;

        internal static Color allyColour;

        internal static Color bothColour;

        internal static bool isColoured;

        internal static readonly List<(StatusEffectBombard effect, CardContainer container, GameObject obj, bool ally)> bombardInformation = new List<(StatusEffectBombard, CardContainer, GameObject, bool)>();

        internal static bool IsTracked(Entity entity)
        {
            return bombardInformation.Any(info => info.effect?.target == entity);
        }

        internal static bool ShouldColour(bool hasEnemyAndAlly)
        {
            var setting = MainModFile.instance.bombardColors;
            if (setting == MainModFile.BombardSetting.If_Needed)
            {
                return hasEnemyAndAlly;
            }
            return setting == MainModFile.BombardSetting.On;
        }

        internal static bool ShouldArrowIn(bool hasEnemyAndAlly, bool multipleBombardiersOnTeam)
        {
            var setting = MainModFile.instance.bombardArrowsIn;
            if (setting == MainModFile.BombardSetting.If_Needed)
            {
                return multipleBombardiersOnTeam || (hasEnemyAndAlly && !ShouldColour(hasEnemyAndAlly));
            }
            return setting == MainModFile.BombardSetting.On;
        }

        internal static bool ShouldArrowOut(bool hasEnemyAndAlly, bool multipleBombardiersOnTeam)
        {
            var setting = MainModFile.instance.bombardArrowsOut;
            if (setting == MainModFile.BombardSetting.If_Needed)
            {
                return multipleBombardiersOnTeam || (hasEnemyAndAlly && !ShouldColour(hasEnemyAndAlly));
            }
            return setting == MainModFile.BombardSetting.On;
        }

        internal static void SetCurrent(StatusEffectBombard bombard)
        {
            current = bombard;
            isAlly = bombard.target?.owner == References.Player;
        }

        internal static void ResolveTargets(CardContainer slot)
        {
            BombardArrowSystem.UpdateContainer(slot);
            bool hasEnemyBombardier = bombardInformation.Any(info => !info.ally);
            bool hasAllyBombardier = bombardInformation.Any(info => info.ally);

            if (ShouldColour(hasEnemyBombardier && hasAllyBombardier))
            {
                isColoured = true;
                foreach (var (_, container, obj, ally) in bombardInformation)
                {
                    bool targetedByBothTeams = bombardInformation.Any(info => info.container == container && info.ally != ally);
                    if (targetedByBothTeams)
                    {
                        SetColour(obj, bothColour);
                    }
                    else
                    {
                        SetColour(obj, ally ? allyColour : originalColour);
                    }
                }
            }
            else if (isColoured)
            {
                isColoured = false;
                foreach (var (_, _, obj, _) in bombardInformation)
                {
                    SetColour(obj, originalColour);
                }
            }
        }

        private static void SetColour(GameObject obj, Color colour)
        {
            SpriteRenderer renderer = GetRenderer(obj);
            if (renderer)
            {
                renderer.color = colour;
            }
        }

        internal static SpriteRenderer GetRenderer(GameObject obj)
        {
            return obj?.transform.GetComponentInChildren<SpriteRenderer>();
        }

        internal static void OnDiscard(Entity entity)
        {
            foreach (var item in entity?.statusEffects)
            {
                if (item is StatusEffectBombard)
                {
                    item.RunDisableEvent(entity);
                }
            }
        }

        internal class BombardArrowSystem
        {
            private static GameObject system;

            private static GameObject asset;

            private static readonly List<GameObject> inArrows = new List<GameObject>();

            private static readonly List<GameObject> outArrows = new List<GameObject>();

            internal static bool HasInArrows { get => inArrows.Count > 0; }

            internal static bool HasOutArrows { get => outArrows.Count > 0; }

            private static Entity dragging;

            private static bool Init()
            {
                if (system && asset)
                {
                    return true;
                }

                if (system == null && SceneManager.Loaded.ContainsKey("Campaign"))
                {
                    Scene s = SceneManager.Loaded["Campaign"];
                    GameObject[] gameObjects = s.GetRootGameObjects();
                    foreach (var item in gameObjects)
                    {
                        if (item.name == "Systems")
                        {
                            system = item;
                            break;
                        }
                    }
                }

                if (asset == null)
                {
                    if (system)
                    {
                        foreach (var item in system.transform.GetAllChildren())
                        {
                            if (item.gameObject.TryGetComponent<TargetingArrow>(out TargetingArrow _))
                            {
                                asset = item.gameObject;
                                break;
                            }
                        }
                    }
                }

                return system && asset;
            }

            internal static void Update()
            {
                if (dragging && !dragging.dragging)
                {
                    OnHoverEntity(dragging);
                    dragging = null;
                }
            }

            internal static void OnDrag(Entity entity)
            {
                dragging = entity;
                CleanUp();
            }

            internal static void OnHoverEntity(Entity entity)
            {
                if (Init() && !SkipArrows() && entity)
                {
                    var found = bombardInformation.Where(info => info.effect?.target == entity).FirstOrDefault();
                    if (found.effect?.target == entity)
                    {
                        bool multipleBombardiers = bombardInformation.Any(info => (info.effect?.target ?? entity) != entity && info.ally == found.ally);
                        bool hasEnemyAndAlly = bombardInformation.Any(info => info.ally != found.ally);
                        if (ShouldArrowOut(hasEnemyAndAlly, multipleBombardiers))
                        {
                            foreach (var (effect, container, _, _) in bombardInformation)
                            {
                                if (effect?.target == entity && container)
                                {
                                    MakeArow(entity.transform.position, container.transform.position, false);
                                }
                            }
                        }
                    }

                    if (HasOutArrows)
                    {
                        if (entity.display?.hover?.pop?.popped ?? false)
                        {
                            entity.display.hover.pop.UnPop();
                        }
                    }
                }
            }

            internal static void OnUnHoverEntity(Entity entity)
            {
                foreach (var item in outArrows)
                {
                    item?.Destroy();
                }
                outArrows.Clear();
            }

            internal static void OnHoverSlot(CardContainer cont)
            {
                if (Init() && !SkipArrows() && cont)
                {
                    int allyCount = bombardInformation.Where(info => info.ally && info.effect?.target).Select(info => info.effect.target).Distinct().Count();
                    int enemyCount = bombardInformation.Where(info => !info.ally && info.effect?.target).Select(info => info.effect.target).Distinct().Count();
                    bool hasEnemyAndAlly = allyCount > 0 && enemyCount > 0;
                    foreach (var (effect, container, _, ally) in bombardInformation)
                    {
                        if (effect && effect.target && container == cont && ShouldArrowIn(hasEnemyAndAlly, (allyCount > 1 && ally) || (enemyCount > 1 && !ally)))
                        {
                            MakeArow(effect.target.transform.position, cont.transform.position, true);
                        }
                    }
                }
            }

            private static bool SkipArrows()
            {
                if (Deckpack.IsOpen || InspectSystem.IsActive() || dragging)
                {
                    return true;
                }
                return false;
            }

            private static void MakeArow(Vector3 from, Vector3 to, bool isInArrow)
            {
                GameObject arrowObj = UnityEngine.Object.Instantiate(asset, system.transform);
                arrowObj.name = asset.name;
                arrowObj.SetActive(true);
                TargetingArrow arrow = arrowObj.GetComponent<TargetingArrow>();
                arrow.SetStyle("Default");
                Vector3 offset = to - from;
                offset *= 0.1f;
                arrow.UpdatePosition(from, to - offset);
                if (isInArrow)
                {
                    inArrows.Add(arrowObj);
                }
                else
                {
                    outArrows.Add(arrowObj);
                }
            }

            internal static void OnUnHoverSlot(CardContainer _)
            {
                foreach (var item in inArrows)
                {
                    item?.Destroy();
                }
                inArrows.Clear();
            }

            internal static void UpdateContainer(CardContainer cont)
            {
                if (HasInArrows)
                {
                    OnUnHoverSlot(cont);
                    OnHoverSlot(cont);
                }
            }

            internal static void OnInspect(Entity _)
            {
                CleanUp();
            }

            internal static void CleanUp()
            {
                foreach (var item in inArrows)
                {
                    item?.Destroy();
                }
                inArrows.Clear();

                foreach (var item in outArrows)
                {
                    item?.Destroy();
                }
                outArrows.Clear();
            }

            internal static void OnSceneChange(Scene _)
            {
                Uninitialize();
            }

            internal static void Uninitialize()
            {
                CleanUp();
                system = null;
                asset = null;
                bombardInformation.Clear();
            }
        }


        [HarmonyPatch(typeof(StatusEffectBombard), nameof(StatusEffectBombard.SetTargets))]
        internal class SetTargetsPatch
        {
            static void Prefix(StatusEffectBombard __instance)
            {
                SetCurrent(__instance);
            }
        }

        [HarmonyPatch(typeof(StatusEffectBombard), nameof(StatusEffectBombard.RunEndEvent))]
        internal class RunEndEventPatch
        {
            static void Prefix(StatusEffectBombard __instance)
            {
                SetCurrent(__instance);
            }
        }

        [HarmonyPatch(typeof(StatusEffectBombard), nameof(StatusEffectBombard.RunDisableEvent))]
        internal class RunDisableEventPatch
        {
            static void Prefix(StatusEffectBombard __instance, Entity entity)
            {
                if (entity == __instance?.target)
                {
                    SetCurrent(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(AbilityTargetSystem), nameof(AbilityTargetSystem.AddTarget))]
        internal class AddTargetPatch
        {
            static void Postfix(AbilityTargetSystem __instance, CardContainer container)
            {
                GameObject obj = __instance?.currentTargets?[container];
                if (obj)
                {
                    bombardInformation.Add((current, container, obj, isAlly));
                    if (initColours)
                    {
                        ResolveTargets(container);
                    }
                    else
                    {
                        SpriteRenderer renderer = GetRenderer(obj);
                        if (renderer)
                        {
                            initColours = true;
                            originalColour = renderer.color;
                            allyColour = new Color(originalColour.g, originalColour.r, originalColour.b, originalColour.a);
                            bothColour = new Color(originalColour.r, originalColour.r, originalColour.b, originalColour.a);
                            ResolveTargets(container);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(AbilityTargetSystem), nameof(AbilityTargetSystem.RemoveTarget))]
        internal class RemoveTargetPatch
        {
            static bool Prefix(AbilityTargetSystem __instance, CardContainer container)
            {
                var found = bombardInformation.Where(info => info.effect == current && info.container == container).FirstOrDefault();
                if (found == default)
                {
                    // If we arent tracking it, let ATS handle it
                    return true;
                }

                // Untrack as its going to be destroyed
                bombardInformation.Remove(found);
                ResolveTargets(container);

                // If ATS is going to destroy the correct one, let it
                if (__instance.currentTargets?.ContainsKey(container) ?? false)
                {
                    GameObject tocheck = __instance.currentTargets[container];
                    if (found.obj == tocheck)
                    {
                        return true;
                    }
                }

                // Else handle it manually
                found.obj?.Destroy();
                return false;
            }
        }

        [HarmonyPatch(typeof(AbilityTargetSystem), nameof(AbilityTargetSystem.Clear))]
        internal class ClearPatch
        {
            static void Postfix()
            {
                foreach (var (_, _, obj, _) in bombardInformation)
                {
                    obj?.Destroy();
                }
                bombardInformation.Clear();
                BombardArrowSystem.CleanUp();
            }
        }

        [HarmonyPatch(typeof(CardPopUpTarget), nameof(CardPopUpTarget.Pop))]
        internal class PopPatch
        {
            static bool Prefix()
            {
                return !BombardArrowSystem.HasOutArrows;
            }
        }
    }
}
