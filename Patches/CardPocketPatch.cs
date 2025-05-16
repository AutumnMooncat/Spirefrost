using HarmonyLib;

namespace Spirefrost.Patches
{
    [HarmonyPatch(typeof(CardPocket), "CardAdded")]
    internal class CardPocketPatch
    {
        static void Postfix(Entity entity)
        {
            entity.enabled = true;
        }
    }
}
