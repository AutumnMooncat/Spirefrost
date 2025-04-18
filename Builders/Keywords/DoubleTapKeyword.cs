using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.Keywords
{
    internal class DoubleTapKeyword : SpirefrostBuilder
    {
        internal static string ID => "stsdoubletap";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Double Tap")
                .WithDescription("Trigger additional times | Clears after triggering")
                .WithTitleColour(new Color(0.55f, 0.1f, 0.1f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.54f, 0.09f, 0.09f))
                .WithCanStack(true);
        }
    }
}
