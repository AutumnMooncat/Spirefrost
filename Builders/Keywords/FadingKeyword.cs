using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.Keywords
{
    internal class FadingKeyword : SpirefrostBuilder
    {
        internal static string ID => "stsfading";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Fading")
                .WithDescription("Die when removed | Counts down every turn after health reaches 0")
                .WithTitleColour(new Color(0.35f, 0.3f, 0.45f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.34f, 0.29f, 0.44f))
                .WithCanStack(true);
        }
    }
}
