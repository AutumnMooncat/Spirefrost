using HarmonyLib;

namespace Spirefrost.Patches
{
    [HarmonyPatch(typeof(StatusEffectData), "RemoveStacks")]
    internal static class RemoveStacksPatch
    {
        static void Prefix(StatusEffectData __instance, ref int amount, bool removeTemporary)
        {
            SpirefrostEvents.InvokePreStatusReduction(__instance, ref amount, removeTemporary);
        }
    }

    // IEnumerator, but we only want Pre and Post so it should be fine
    [HarmonyPatch(typeof(StatusEffectSystem), "Apply")]
    internal static class StatusSystemPatch
    {
        internal static bool isTemp;

        static void Prefix(StatusEffectSystem __instance, bool temporary)
        {
            isTemp = temporary;
        }

        static void Postfix(StatusEffectSystem __instance) 
        {
            isTemp = false;
        }
    }
}
