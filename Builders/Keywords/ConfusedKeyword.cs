using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.Keywords
{
    internal class ConfusedKeyword : SpirefrostBuilder
    {
        internal static string ID => "stsconfused";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Confused")
                .WithDescription("When attacking, hit a random ally instead | Counts down after each attack")
                .WithTitleColour(new Color(1.0f, 0.8f, 0.15f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.99f, 0.79f, 0.14f))
                .WithCanStack(true);
        }
    }
}
