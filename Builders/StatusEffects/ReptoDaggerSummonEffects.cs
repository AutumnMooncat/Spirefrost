using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Cards.Items;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenDeployedAddReptoDaggerToHand : SpirefrostBuilder
    {
        internal static string ID => "When Deployed Add Repto Dagger To Hand";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Deployed Add Junk To Hand", ID)
                .WithText("When deployed, add <{a}> {0} to your hand")
                .WithTextInsert(MakeCardInsert(ReptoDagger.FullID))
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenDeployed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(InstantSummonReptoDaggerInHand.ID);
                });
        }
    }

    internal class InstantSummonReptoDaggerInHand : SpirefrostBuilder
    {
        internal static string ID => "Instant Summon Repto Dagger In Hand";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Instant Summon Gearhammer In Hand", ID)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantSummon>(data =>
                {
                    data.targetSummon = (StatusEffectSummon)TryGet<StatusEffectData>(SummonReptoDagger.ID);
                });
        }
    }

    internal class SummonReptoDagger : SpirefrostBuilder
    {
        internal static string ID => "Summon Repto Dagger";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Summon Gearhammer", ID)
                .SubscribeToAfterAllBuildEvent<StatusEffectSummon>(data =>
                {
                    data.summonCard = TryGet<CardData>(ReptoDagger.ID);
                });
        }
    }
}
