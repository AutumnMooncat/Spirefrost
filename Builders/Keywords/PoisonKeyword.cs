using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.Keywords
{
    internal class PoisonKeyword : SpirefrostBuilder
    {
        internal static string ID => "stspoison";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Poison")
                .WithDescription("Deals damage every turn | Counts down every turn")
                .WithTitleColour(new Color(0.5f, 1.0f, 0.3f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.49f, 0.99f, 0.29f))
                .WithCanStack(true);
        }
    }
}
