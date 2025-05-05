using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects.IconEffects;
using UnityEngine;

namespace Spirefrost.Builders.Keywords
{
    internal class LightningKeyword : SpirefrostBuilder
    {
        internal static string ID => "stslightning";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Lightning Orb")
                .WithDescription($"Passive: Deals {LightningOrb.ApplyAmount} damage to a random enemy every turn\n\nEvoke: Deals twice as much damage to a random enemy | Clears and Evokes before triggering")
                .WithTitleColour(new Color(0.95f, 0.95f, 0.05f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.94f, 0.94f, 0.04f))
                .WithCanStack(false);
        }
    }
}
