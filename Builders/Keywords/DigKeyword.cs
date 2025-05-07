using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.Keywords
{
    internal class DigKeyword : SpirefrostBuilder
    {
        internal static string ID => "stsdig";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Dig")
                .WithDescription("Put a random <Item> from your draw pile into your hand")
                .WithTitleColour()
                .WithBodyColour()
                .WithNoteColour()
                .WithCanStack(true)
                .WithShowName(true);
        }
    }
}
