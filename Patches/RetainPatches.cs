using HarmonyLib;
using System.Collections.Generic;


namespace Spirefrost.Patches
{
    [HarmonyPatch(typeof(ActionRedraw), "DiscardAll")]
    internal static class RetainPatches
    {
        private static readonly List<Entity> retainedCards = new List<Entity>();

        static void Prefix()
        {
            retainedCards.Clear();
            foreach (Entity item in References.Player.handContainer)
            {
                if (item.statusEffects.Exists(effect => effect is StatusEffectRetain))
                {
                    retainedCards.Add(item);
                }
            }

            References.Player.handContainer.RemoveMany(retainedCards);
        }

        static void Postfix()
        {
            foreach (Entity item in retainedCards)
            {
                References.Player.handContainer.Add(item);
            }

            SpirefrostEvents.InvokeCardsRetained(retainedCards);
        }
    }
}
