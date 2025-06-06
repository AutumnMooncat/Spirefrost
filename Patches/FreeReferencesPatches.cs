using HarmonyLib;

namespace Spirefrost.Patches
{
    [HarmonyPatch]
    internal class FreeReferencesPatches
    {
        [HarmonyPatch(typeof(Entity), nameof(Entity.OnReturnToPool))]
        internal static class EntityToPoolPatch
        {
            static void Prefix(Entity __instance)
            {
                SpirefrostUtils.FreeReferences(__instance);
            }
        }

        [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), typeof(UnityEngine.Object))]
        internal static class ObjectDestroyPatch
        {
            static void Prefix(UnityEngine.Object obj)
            {
                SpirefrostUtils.FreeReferences(obj);
            }
        }
    }
}
