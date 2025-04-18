using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects.IconEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhileActiveRitualToAlliesInRow : SpirefrostBuilder
    {
        internal static string ID => "While Active STS Ritual To AlliesInRow";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("While Active Frenzy To AlliesInRow", ID)
                .WithText($"While active, add <{{a}}>{MakeKeywordInsert(RitualKeyword.FullID)} to allies in the row")
                .SubscribeToAfterAllBuildEvent<StatusEffectWhileActiveX>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(Ritual.ID);
                });
        }
    }
}
