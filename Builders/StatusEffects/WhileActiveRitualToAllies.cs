using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects.IconEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhileActiveRitualToAllies : SpirefrostBuilder
    {
        internal static string ID => "While Active STS Ritual To Allies";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("While Active Teeth To Allies", ID)
                .WithText($"While active, add <{{a}}>{MakeKeywordInsert(RitualKeyword.FullID)} to all allies")
                .SubscribeToAfterAllBuildEvent<StatusEffectWhileActiveX>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(Ritual.ID);
                });
        }
    }
}
