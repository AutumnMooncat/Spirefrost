using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.Keywords
{
    internal class ShackledKeyword : SpirefrostBuilder
    {
        internal static string ID => "stsshackled";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Shackled")
                .WithDescription("Temporarily reduces attack | Clears after triggering")
                .WithTitleColour(new Color(0.7f, 0.85f, 0.8f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.69f, 0.84f, 0.79f))
                .WithCanStack(true);
        }
    }
}
