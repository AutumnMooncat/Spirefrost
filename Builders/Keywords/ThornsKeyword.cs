using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.Keywords
{
    internal class ThornsKeyword : SpirefrostBuilder
    {
        internal static string ID => "ststhorns";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Thorns")
                .WithDescription("Deals damage to attackers")
                .WithTitleColour(new Color(0.6f, 0.7f, 0.75f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.59f, 0.69f, 0.74f))
                .WithCanStack(true);
        }
    }
}
