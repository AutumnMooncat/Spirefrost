using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.Keywords
{
    internal class AmplifyKeyword : SpirefrostBuilder
    {
        internal static string ID => "stsamplify";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Amplify")
                .WithDescription("Increases target's effects | Clears after triggering")
                .WithTitleColour(new Color(0.25f, 0.75f, 0.85f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.24f, 0.74f, 0.84f))
                .WithCanStack(true);
        }
    }
}
