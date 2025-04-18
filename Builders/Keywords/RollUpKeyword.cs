using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.Keywords
{
    internal class RollUpKeyword : SpirefrostBuilder
    {
        internal static string ID => "stsrollup";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Roll Up")
                .WithDescription("When hit, gain <keyword=shell> and lose Roll Up")
                .WithTitleColour(new Color(0.35f, 0.7f, 0.8f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.34f, 0.69f, 0.79f))
                .WithCanStack(true);
        }
    }
}
