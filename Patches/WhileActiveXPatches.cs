using HarmonyLib;
using System.Linq;

namespace Spirefrost.Patches
{
    [HarmonyPatch]
    internal class WhileActiveXPatches
    {
        [HarmonyPatch(typeof(StatusEffectWhileActiveX), nameof(StatusEffectWhileActiveX.FindContainers))]
        internal static class FindContainersPatch
        {
            static void Postfix(StatusEffectWhileActiveX __instance)
            {
                if (__instance.AppliesTo(StatusEffectApplyX.ApplyToFlags.FrontEnemy))
                {
                    foreach (int rowIndex2 in Battle.instance.GetRowIndices(__instance.target))
                    {
                        CardSlotLane current = Battle.instance.GetRow(__instance.target.owner, rowIndex2) as CardSlotLane;
                        if (current)
                        {
                            CardSlot value = Battle.instance.GetOppositeRow(current).slots.FirstOrDefault((CardSlot a) => !a.Empty);
                            if (!__instance.containersToAffect.Contains(value))
                            {
                                __instance.containersToAffect.AddIfNotNull(value);
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(StatusEffectWhileActiveX), nameof(StatusEffectWhileActiveX.RunEntityDestroyedEvent))]
        internal static class RunEntityDestroyedEventPatch
        {
            static void Prefix(StatusEffectWhileActiveX __instance, Entity entity)
            {
                if (__instance.affected.Contains(entity))
                {
                    ActionQueue.Add(new ActionRefreshWhileActiveEffect(__instance));
                }
            }
        }
    }
}
