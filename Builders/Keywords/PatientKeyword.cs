using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.Keywords
{
    internal class PatientKeyword : SpirefrostBuilder
    {
        internal static string ID => "patient";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new KeywordDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithTitle("Patient")
                .WithDescription("When <Retained>, gain <keyword=zoomlin>")
                .WithTitleColour()
                .WithBodyColour()
                .WithNoteColour()
                .WithShowName(true);
        }
    }
}
