using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.Keywords
{
    internal class DivideKeyword : SpirefrostBuilder
    {
        internal static string ID => "stsdivide";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Divide")
                .WithDescription("Break in to 2 cards with equal <keyword=health> and status effects")
                .WithTitleColour()
                .WithBodyColour()
                .WithNoteColour()
                .WithShowName(true);
        }
    }
}
