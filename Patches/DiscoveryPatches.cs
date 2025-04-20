using HarmonyLib;
using UnityEngine;


namespace Spirefrost.Patches
{
    [HarmonyPatch(typeof(CardPocketSequence), "Move")]
    internal static class DiscoveryPatches
    {
        internal static bool instantSnap;

        static bool Prefix(CardPocketSequence __instance, Entity entity, bool includeRandomness)
        {
            if (instantSnap)
            {
                if (entity.transform.parent == __instance.container.holder)
                {
                    Vector3 childPosition = __instance.container.GetChildPosition(entity);
                    Vector3 childRotation = __instance.container.GetChildRotation(entity);
                    Vector3 childScale = __instance.container.GetChildScale(entity);
                    entity.transform.localPosition = childPosition;
                    entity.transform.localEulerAngles = childRotation;
                    entity.transform.localScale = childScale;
                    return false;
                }
            }
            return true;
        }
    }
}
