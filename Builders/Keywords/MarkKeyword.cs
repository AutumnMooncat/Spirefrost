using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.Keywords
{
    internal class MarkKeyword : SpirefrostBuilder
    {
        internal static string ID => "stsmark";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Mark")
                .WithDescription("When applied, all enemies take damage equal to their Mark | Does not count down!")
                .WithTitleColour(new Color(0.4f, 0.7f, 0.7f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.39f, 0.69f, 0.69f))
                .WithCanStack(true);
        }
    }
}
