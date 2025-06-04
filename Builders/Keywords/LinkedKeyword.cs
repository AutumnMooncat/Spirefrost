using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.Keywords
{
    internal class LinkedKeyword : SpirefrostBuilder
    {
        internal static string ID => "linked";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Linked")
                .WithDescription("When this card is chosen, also obtain the following cards")
                .WithTitleColour()
                .WithBodyColour()
                .WithNoteColour()
                .WithShowName(true);
        }
    }
}
