using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.Keywords
{
    internal class FlightKeyword : SpirefrostBuilder
    {
        internal static string ID => "stsflight";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Flight")
                .WithDescription("Halves damage taken | Counts down after taking damage")
                .WithTitleColour(new Color(0.7f, 0.8f, 0.9f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.69f, 0.79f, 0.89f))
                .WithCanStack(true);
        }
    }
}
