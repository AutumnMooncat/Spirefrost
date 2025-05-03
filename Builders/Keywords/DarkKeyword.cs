using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects.IconEffects;
using UnityEngine;

namespace Spirefrost.Builders.Keywords
{
    internal class DarkKeyword : SpirefrostBuilder
    {
        internal static string ID => "stsdark";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Dark Orb")
                .WithDescription($"Stores {DarkOrb.ScaleAmount} damage every turn\n\nAfter triggering, damages all enemies in the row | Retargets if row empty\n\nClears after triggering")
                .WithTitleColour(new Color(0.50f, 0.40f, 0.80f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.49f, 0.39f, 0.79f))
                .WithCanStack(false);
        }
    }
}
