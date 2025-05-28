using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.Keywords
{
    internal class SkillKeyword : SpirefrostBuilder
    {
        internal static string ID => "stsskill";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Skill")
                .WithDescription("An <Item> without <keyword=attack>")
                .WithTitleColour()
                .WithBodyColour()
                .WithNoteColour()
                .WithShowName(true);
        }
    }
}
