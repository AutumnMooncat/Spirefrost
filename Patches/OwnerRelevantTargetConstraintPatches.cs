using Deadpan.Enums.Engine.Components.Modding;
using HarmonyLib;
using System.Collections;

namespace Spirefrost.Patches
{
    [HarmonyPatch(typeof(StatusEffectApplyX), "Run")]
    internal static class RunPatch
    {
        static void Prefix(StatusEffectApplyX __instance)
        {
            foreach (TargetConstraint tc in __instance.applyConstraints)
            {
                if (tc is OwnerRelevantTargetConstraint ortc)
                {
                    //MainModFile.Print($"OwnerRelevantTargetConstraint set to {__instance.target}");
                    ortc.relevantEntity = __instance.target;
                }
            }
        }

        static IEnumerator Postfix(IEnumerator values, StatusEffectApplyX __instance)
        {
            while (values != null && values.MoveNext())
            {
                yield return values.Current;
            }

            foreach (TargetConstraint tc in __instance.applyConstraints)
            {
                if (tc is OwnerRelevantTargetConstraint ortc)
                {
                    //MainModFile.Print($"OwnerRelevantTargetConstraint set to null");
                    ortc.relevantEntity = null;
                }
            }
        }
    }

    [HarmonyPatch(typeof(StatusEffectApplyX), "CheckConstraints")]
    internal static class CheckConstraintsPatch
    {
        static void Prefix(StatusEffectApplyX __instance)
        {
            foreach (TargetConstraint tc in __instance.applyConstraints)
            {
                if (tc is OwnerRelevantTargetConstraint ortc)
                {
                    //MainModFile.Print($"OwnerRelevantTargetConstraint set to {__instance.target}");
                    ortc.relevantEntity = __instance.target;
                }
            }
        }

        static void Postfix(StatusEffectApplyX __instance)
        {
            foreach (TargetConstraint tc in __instance.applyConstraints)
            {
                if (tc is OwnerRelevantTargetConstraint ortc)
                {
                    //MainModFile.Print($"OwnerRelevantTargetConstraint set to null");
                    ortc.relevantEntity = null;
                }
            }
        }
    }
}
