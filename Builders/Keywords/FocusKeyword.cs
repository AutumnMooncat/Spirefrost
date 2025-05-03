using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.Keywords
{
    internal class FocusKeyword : SpirefrostBuilder
    {
        internal static string ID => "stsfocus";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Focus")
                .WithDescription("When a positive status effect is gained, increase its amount")
                .WithTitleColour()
                .WithBodyColour()
                .WithNoteColour()
                .WithCanStack(true)
                .WithShowName(true);
        }
    }
}
