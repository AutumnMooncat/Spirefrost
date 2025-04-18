using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Cards.Items;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnCardPlayedAddShivToHand : SpirefrostBuilder
    {
        internal static string ID => "On Card Played Add Shiv To Hand";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Card Played Add Gearhammer To Hand", ID)
                .WithText("Add <{a}> {0} to your hand")
                .WithTextInsert(MakeCardInsert(Shiv.FullID))
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(InstantSummonShivInHand.ID);
                });
        }
    }

    internal class InstantSummonShivInHand : SpirefrostBuilder
    {
        internal static string ID => "Instant Summon Shiv In Hand";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Instant Summon Gearhammer In Hand", ID)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantSummon>(data =>
                {
                    data.targetSummon = (StatusEffectSummon)TryGet<StatusEffectData>(SummonShiv.ID);
                });
        }
    }

    internal class SummonShiv : SpirefrostBuilder
    {
        internal static string ID => "Summon Shiv";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Summon Gearhammer", ID)
                .SubscribeToAfterAllBuildEvent<StatusEffectSummon>(data =>
                {
                    data.summonCard = TryGet<CardData>(Shiv.ID);
                });
        }
    }
}
