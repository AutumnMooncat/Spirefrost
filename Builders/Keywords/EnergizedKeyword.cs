using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.Keywords
{
    internal class EnergizedKeyword : SpirefrostBuilder
    {
        internal static string ID => "stsenergized";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Energized")
                .WithDescription("Counts down Counters<sprite name=counter> when they are reset from triggering | Clears after activation")
                .WithTitleColour(new Color(0.25f, 0.7f, 0.95f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.24f, 0.69f, 0.94f))
                .WithCanStack(true);
        }
    }
}
