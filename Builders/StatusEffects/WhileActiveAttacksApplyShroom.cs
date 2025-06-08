using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhileActiveAttacksApplyShroom : SpirefrostBuilder
    {
        internal static string ID => "While Active Items Apply Shroom";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("While Active Increase Attack To Items In Hand", ID)
                .WithText($"While active, add <+{{a}}><keyword=shroom> to {MakeKeywordInsert(AttackKeyword.FullID)}<s >in your hand")
                .SubscribeToAfterAllBuildEvent<StatusEffectWhileActiveX>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(OngoingApplyShroom.ID);
                    data.WithSwappable(TryGet<StatusEffectData>("On Card Played Apply Shroom To Enemies"));
                });
        }
    }

    internal class OngoingApplyShroom : SpirefrostBuilder
    {
        internal static string ID => "Ongoing Apply Shroom";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectOngoingAttackEffect>(ID)
                .SubscribeToAfterAllBuildEvent<StatusEffectOngoingAttackEffect>(data =>
                {
                    data.effect = TryGet<StatusEffectData>("Shroom");
                });
        }
    }
}
