using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.Keywords
{
    internal class ConstrictedKeyword : SpirefrostBuilder
    {
        internal static string ID => "stsconstricted";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Constricted")
                .WithDescription("Deals damage every turn | Clears when applier is destroyed")
                .WithTitleColour(new Color(0.5f, 0.8f, 0.85f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.49f, 0.84f, 0.84f))
                .WithCanStack(true);
        }
    }
}
