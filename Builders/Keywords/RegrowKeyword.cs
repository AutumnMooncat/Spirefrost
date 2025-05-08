using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.Keywords
{
    internal class RegrowKeyword : SpirefrostBuilder
    {
        internal static string ID => "stsregrow";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Life Link")
                .WithDescription("When killed, <keyword=cleanse> and revive with half <keyword=health> | Counts down after activation")
                .WithTitleColour(new Color(0.85f, 0.75f, 0.25f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.84f, 0.74f, 0.24f))
                .WithCanStack(true);
        }
    }
}
