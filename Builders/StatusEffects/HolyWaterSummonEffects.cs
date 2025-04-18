using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Cards.Items;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnCardPlayedAddHolyWaterToHand : SpirefrostBuilder
    {
        internal static string ID => "On Card Played Add Holy Water To Hand";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Card Played Add Gearhammer To Hand", ID)
                .WithText("Add <{a}> {0} to your hand")
                .WithTextInsert(MakeCardInsert(HolyWater.FullID))
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(InstantSummonHolyWaterInHand.ID);
                });
        }
    }

    internal class InstantSummonHolyWaterInHand : SpirefrostBuilder
    {
        internal static string ID => "Instant Summon Holy Water In Hand";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Instant Summon Gearhammer In Hand", ID)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantSummon>(data =>
                {
                    data.targetSummon = (StatusEffectSummon)TryGet<StatusEffectData>(SummonHolyWater.ID);
                });
        }
    }

    internal class SummonHolyWater : SpirefrostBuilder
    {
        internal static string ID => "Summon Holy Water";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Summon Gearhammer", ID)
                .SubscribeToAfterAllBuildEvent<StatusEffectSummon>(data =>
                {
                    data.summonCard = TryGet<CardData>(HolyWater.ID);
                });
        }
    }
}
