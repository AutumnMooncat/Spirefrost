using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.Keywords
{
    internal class RitualKeyword : SpirefrostBuilder
    {
        internal static string ID => "stsritual";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Ritual")
                .WithDescription("When <Redraw Bell> is hit, gain <keyword=attack>")
                .WithTitleColour(new Color(0.5f, 0.8f, 1.0f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.49f, 0.79f, 0.99f))
                .WithCanStack(true);
        }
    }
}
