using HarmonyLib;
using Spirefrost.StatusEffects;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static Targets;

namespace Spirefrost.Patches
{
    [HarmonyPatch]
    internal class ExtraCounterPatches
    {
        internal static List<StatusIconCounter> GetCustomCounterIcons(Entity entity)
        {
            return entity.statusEffects
                .Select(effect => effect as INonStackingStatusEffect)
                .Where(effect => effect != null && effect.Icon is StatusIconCounter counter)
                .Select(effect => effect.Icon as StatusIconCounter)
                .ToList();
        }

        internal static StatusEffectExtraCounter NextCustomCounterAtZero(Entity entity)
        {
            return GetCustomCounterIcons(entity).Select(icon => icon.GetLinkedStatus()).Where(status => status is StatusEffectExtraCounter && status.count == 0).FirstOrDefault() as StatusEffectExtraCounter;
        }

        [HarmonyPatch(typeof(StatusEffectInstantReduceMaxCounter), nameof(StatusEffectInstantReduceMaxCounter.Process))]
        internal class ReduceMaxCounterPatch
        {
            static void Prefix(StatusEffectInstantReduceMaxCounter __instance)
            {
                foreach (var item in __instance.target.statusEffects)
                {
                    if (item is StatusEffectExtraCounter extra)
                    {
                        extra.ModifyMaxCounter(-__instance.GetAmount());
                    }
                }
            }
        }

        [HarmonyPatch(typeof(StatusEffectInstantIncreaseMaxCounter), nameof(StatusEffectInstantIncreaseMaxCounter.Process))]
        internal class IncreaseMaxCounterPatch
        {
            static void Prefix(StatusEffectInstantIncreaseMaxCounter __instance)
            {
                foreach (var item in __instance.target.statusEffects)
                {
                    if (item is StatusEffectExtraCounter extra)
                    {
                        extra.ModifyMaxCounter(__instance.GetAmount());
                    }
                }
            }
        }

        [HarmonyPatch(typeof(StatusEffectInstantRandomizeStats), nameof(StatusEffectInstantRandomizeStats.Process))]
        internal class RandomizeStatsPatch
        {
            static void Prefix(StatusEffectInstantRandomizeStats __instance)
            {
                foreach (var item in __instance.target.statusEffects)
                {
                    if (item is StatusEffectExtraCounter extra)
                    {
                        extra.ModifyMaxCounter(Dead.Random.Range(__instance.min, __instance.max), true);
                    }
                }
            }
        }

        [HarmonyPatch]
        internal class CounterImminentPatches
        {
            internal static bool CounterCheck(bool enable, Entity entity)
            {
                return enable && entity.counter.current == 1;
            }

            [HarmonyPatch(typeof(CounterImminentDisplaySystem), nameof(CounterImminentDisplaySystem.EntityCheck))]
            internal static class EntityCheckPatch
            {
                static void Postfix(CounterImminentDisplaySystem __instance, Entity entity)
                {
                    // If we are imminent but have multiple counters, we need to recall set to ensure the correct counter is the one animating
                    List<StatusIconCounter> counters = GetCustomCounterIcons(entity);
                    if (entity.enabled && counters.Count > 0 && __instance.Imminent(entity))
                    {
                        __instance.SetCounterIconAnimation(entity, true);
                    }
                }
            }

            [HarmonyPatch(typeof(CounterImminentDisplaySystem), nameof(CounterImminentDisplaySystem.Imminent))]
            internal static class ImminentPatch
            {
                static void Postfix(Entity entity, ref bool __result)
                {
                    if (!entity.IsSnowed && GetCustomCounterIcons(entity).Any(icon => icon.GetValue().current == 1))
                    {
                        __result = true;
                    }
                }
            }

            [HarmonyPatch(typeof(CounterImminentDisplaySystem), nameof(CounterImminentDisplaySystem.SetCounterIconAnimation))]
            internal static class SetCounterIconAnimationPatch
            {
                static void Postfix(Entity entity, bool enable)
                {
                    foreach (var item in GetCustomCounterIcons(entity))
                    {
                        CardIdleAnimation imminentAnimation = item.imminentAnimation;
                        if (imminentAnimation != null)
                        {
                            if (enable && item.GetValue().current == 1)
                            {
                                imminentAnimation.FadeIn();
                                continue;
                            }
                            imminentAnimation.FadeOut();
                        }
                    }
                }

                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    List<CodeInstruction> codes = instructions.ToList();
                    MethodInfo check = AccessTools.Method(typeof(CounterImminentPatches), nameof(CounterCheck));

                    for (int i = 0; i < codes.Count; i++)
                    {
                        yield return codes[i];

                        if (codes[i].opcode == OpCodes.Ldarg_2)
                        {
                            Debug.Log($"SetCounterIconAnimationPatch found 'enable' load, inserting check");
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Call, check);
                        }
                    }
                }
            }
        }

        [HarmonyPatch]
        internal class CardUpgradePatches
        {
            [HarmonyPatch(typeof(CardUpgradeData), nameof(CardUpgradeData.AdjustStats))]
            internal static class AdjustStatsPatch
            {
                static void Postfix(CardUpgradeData __instance, CardData cardData)
                {
                    foreach (var item in cardData.startWithEffects)
                    {
                        if (item.data is StatusEffectExtraCounter)
                        {
                            item.count = (__instance.setCounter ? __instance.counter : Mathf.Max(1, item.count + __instance.counter));
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(CardUpgradeData), nameof(CardUpgradeData.UnAssign))]
            internal static class UnAssignPatch
            {
                static void Postfix(CardUpgradeData __instance, CardData assignedTo)
                {
                    foreach (var item in assignedTo.startWithEffects)
                    {
                        if (item.data is StatusEffectExtraCounter)
                        {
                            item.count -= __instance.counterChange;
                        }
                    }
                }
            }
        }

        [HarmonyPatch]
        internal class CardScriptPatches
        {
            [HarmonyPatch(typeof(CardScriptAddRandomCounter), nameof(CardScriptAddRandomCounter.Run))]
            internal static class AddRandomCounterPatch
            {
                static void Postfix(CardScriptAddRandomCounter __instance, CardData target)
                {
                    foreach (var item in target.startWithEffects)
                    {
                        if (item.data is StatusEffectExtraCounter)
                        {
                            item.count += __instance.counterRange.Random();
                            item.count = Mathf.Max(1, item.count);
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(CardScriptMultiplyCounter), nameof(CardScriptMultiplyCounter.Run))]
            internal static class MultiplyCounterPatch
            {
                static void Postfix(CardScriptMultiplyCounter __instance, CardData target)
                {
                    foreach (var item in target.startWithEffects)
                    {
                        if (item.data is StatusEffectExtraCounter)
                        {
                            item.count = __instance.roundUp ? Mathf.CeilToInt(item.count * __instance.multiply) : Mathf.RoundToInt(item.count * __instance.multiply);
                            item.count = Mathf.Max(1, item.count);
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(CardScriptSetCounter), nameof(CardScriptSetCounter.Run))]
            internal static class SetCounterPatch
            {
                static void Postfix(CardScriptSetCounter __instance, CardData target)
                {
                    foreach (var item in target.startWithEffects)
                    {
                        if (item.data is StatusEffectExtraCounter)
                        {
                            item.count = __instance.counterRange.Random();
                            item.count = Mathf.Max(1, item.count);
                        }
                    }
                }
            }
        }

        [HarmonyPatch]
        internal class TargetConstraintPatches
        {
            [HarmonyPatch(typeof(TargetConstraintMaxCounterMoreThan), nameof(TargetConstraintMaxCounterMoreThan.Check), typeof(Entity))]
            static class EntityMaxCounterMoreThanPatch
            {
                static void Postfix(TargetConstraintMaxCounterMoreThan __instance, Entity target, ref bool __result)
                {
                    if (__instance.not)
                    {
                        foreach (var item in target.statusEffects)
                        {
                            if (item is StatusEffectExtraCounter counter)
                            {
                                __result &= counter.maxCount <= __instance.moreThan;
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in target.statusEffects)
                        {
                            if (item is StatusEffectExtraCounter counter)
                            {
                                __result |= counter.maxCount > __instance.moreThan;
                            }
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(TargetConstraintMaxCounterMoreThan), nameof(TargetConstraintMaxCounterMoreThan.Check), typeof(CardData))]
            static class CardDataMaxCounterMoreThanPatch
            {
                static void Postfix(TargetConstraintMaxCounterMoreThan __instance, CardData targetData, ref bool __result)
                {
                    if (__instance.not)
                    {
                        foreach (var item in targetData.startWithEffects)
                        {
                            if (item.data is StatusEffectExtraCounter)
                            {
                                __result &= item.count <= __instance.moreThan;
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in targetData.startWithEffects)
                        {
                            if (item.data is StatusEffectExtraCounter counter)
                            {
                                __result |= item.count > __instance.moreThan;
                            }
                        }
                    }
                }
            }
        }
    }
}
