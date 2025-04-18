using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Cards.Companions;

namespace Spirefrost.Builders.StatusEffects
{
    internal class STSSplit : SpirefrostBuilder
    {
        internal static string ID => "STS Split";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Split", ID)
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantSplit>(data =>
                {
                    data.profiles = new StatusEffectInstantSplit.Profile[] {
                        new StatusEffectInstantSplit.Profile()
                        {
                            cardName = SpikeSlime.FullID,
                            changeToCardName = SpikeSlime2.FullID
                        },
                        new StatusEffectInstantSplit.Profile()
                        {
                            cardName = SpikeSlime2.FullID,
                            changeToCardName = SpikeSlime3.FullID
                        }
                    };
                });
        }
    }

    internal class WhenXHealthLostSTSSplit : SpirefrostBuilder
    {
        internal static string ID => "When X Health Lost STS Split";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When X Health Lost Split", ID)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenHealthLost>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(STSSplit.ID);
                });
        }
    }
}
