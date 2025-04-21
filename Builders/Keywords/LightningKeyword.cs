using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.Keywords
{
    internal class LightningKeyword : SpirefrostBuilder
    {
        internal static string ID => "stslightning";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Lightning Orb")
                .WithDescription("Deals damage to a random enemy every turn | Clears after triggering")
                .WithTitleColour(new Color(0.95f, 0.95f, 0.05f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.94f, 0.94f, 0.04f))
                .WithCanStack(false);
        }
    }
}
