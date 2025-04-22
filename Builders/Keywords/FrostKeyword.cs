using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects.IconEffects;
using UnityEngine;

namespace Spirefrost.Builders.Keywords
{
    internal class FrostKeyword : SpirefrostBuilder
    {
        internal static string ID => "stsfrost";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Frost Orb")
                .WithDescription($"Applies {FrostOrb.ApplyAmount}<keyword=frost> to the front enemy every turn | Clears after triggering")
                .WithTitleColour(new Color(0.50f, 0.95f, 0.95f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.49f, 0.94f, 0.94f))
                .WithCanStack(false);
        }
    }
}
