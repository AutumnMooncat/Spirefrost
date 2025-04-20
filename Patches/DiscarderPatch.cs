using HarmonyLib;


namespace Spirefrost.Patches
{
    [HarmonyPatch(typeof(Discarder), "ClearStatusEffects")]
    internal static class DiscarderPatch
    {
        static void Prefix(Discarder __instance, Entity entity)
        {
            SpirefrostEvents.InvokeMovedByDiscarder(entity);
        }
    }
}
