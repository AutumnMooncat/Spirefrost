using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Spirefrost.Patches
{
    [HarmonyPatch]
    internal class BombardPatches
    {
        internal static bool isAlly;

        internal static StatusEffectBombard current;

        internal static bool initColours;

        internal static Color originalColor;

        internal static Color allyColor;

        internal static Color bothColor;

        internal static readonly List<(StatusEffectBombard effect, CardContainer container, GameObject obj, bool ally)> bombardInformation = new List<(StatusEffectBombard, CardContainer, GameObject, bool)>();

        internal static void SetCurrent(StatusEffectBombard bombard)
        {
            current = bombard;
            isAlly = bombard.target?.owner == References.Player;
        }

        internal static void ResolveColours(CardContainer slot)
        {
            bool hasEnemy = bombardInformation.Any(info => info.container == slot && !info.ally);
            bool hasAlly = bombardInformation.Any(info => info.container == slot && info.ally);

            foreach (var (_, container, obj, ally) in bombardInformation)
            {
                if (container == slot)
                {
                    if (hasAlly && hasEnemy)
                    {
                        SetColour(obj, bothColor);
                    }
                    else
                    {
                        SetColour(obj, ally ? allyColor : originalColor);
                    }
                }
            }
        }

        private static void SetColour(GameObject obj, Color color)
        {
            SpriteRenderer renderer = GetRenderer(obj);
            if (renderer)
            {
                renderer.color = color;
            }
        }

        internal static SpriteRenderer GetRenderer(GameObject obj)
        {
            return obj.transform.GetComponentInChildren<SpriteRenderer>();
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
                if (entity == __instance.target)
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
                GameObject obj = __instance.currentTargets?[container];
                if (obj)
                {
                    bombardInformation.Add((current, container, obj, isAlly));
                    if (initColours)
                    {
                        ResolveColours(container);
                    }
                    else
                    {
                        SpriteRenderer renderer = GetRenderer(obj);
                        if (renderer)
                        {
                            initColours = true;
                            originalColor = renderer.color;
                            allyColor = new Color(originalColor.g, originalColor.r, originalColor.b, originalColor.a);
                            bothColor = new Color(originalColor.r, originalColor.r, originalColor.b, originalColor.a);
                            ResolveColours(container);
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
                ResolveColours(container);
                GameObject tocheck = __instance.currentTargets?[container];
                if (found.obj == tocheck)
                {
                    // If ATS is going to remove the correct one, let it
                    return true;
                }
                // Else handle it manually
                found.obj.Destroy();
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
                    obj.Destroy();
                }
                bombardInformation.Clear();
            }
        }
    }
}
