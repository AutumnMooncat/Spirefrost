using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.Keywords
{
    internal class IntangibleKeyword : SpirefrostBuilder
    {
        internal static string ID => "stsintangible";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Intangible")
                .WithDescription("Cannot be targeted by any effects or cards that deal damage")
                .WithTitleColour(new Color(0.35f, 0.65f, 0.45f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.34f, 0.64f, 0.44f))
                .WithCanStack(true);
        }
    }
}
