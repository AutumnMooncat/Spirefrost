using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects.IconEffects;
using UnityEngine;

namespace Spirefrost.Builders.Keywords
{
    internal class PlasmaKeyword : SpirefrostBuilder
    {
        internal static string ID => "stsplasma";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Plasma Orb")
                .WithDescription($"Passive: Counts down <keyword=counter> by {PlasmaOrb.ApplyAmount} every turn\n\nEvoke: Counts down <keyword=counter> of allies in the row by {PlasmaOrb.ApplyAmount} | Clears and Evokes before triggering")
                .WithTitleColour(new Color(0.60f, 1.00f, 0.90f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.59f, 0.99f, 0.99f))
                .WithCanStack(false);
        }
    }
}
