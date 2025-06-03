using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.Keywords
{
    internal class VigorKeyword : SpirefrostBuilder
    {
        internal static string ID => "stsvigor";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Vigor")
                .WithDescription("Temporarily increases attack | Clears after triggering")
                .WithTitleColour(new Color(1.0f, 0.4f, 0.05f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.99f, 0.39f, 0.04f))
                .WithCanStack(true);
        }
    }
}
