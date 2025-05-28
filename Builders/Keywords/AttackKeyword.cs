using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.Keywords
{
    internal class AttackKeyword : SpirefrostBuilder
    {
        internal static string ID => "stsattack";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Attack")
                .WithDescription("An <Item> with <keyword=attack>")
                .WithTitleColour()
                .WithBodyColour()
                .WithNoteColour()
                .WithShowName(true);
        }
    }
}
