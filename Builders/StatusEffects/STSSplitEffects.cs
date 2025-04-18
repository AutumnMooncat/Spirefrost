using Deadpan.Enums.Engine.Components.Modding;

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
                            cardName = "autumnmooncat.wildfrost.spirefrost.spikeslime",
                            changeToCardName = "autumnmooncat.wildfrost.spirefrost.spikeslime2"
                        },
                        new StatusEffectInstantSplit.Profile()
                        {
                            cardName = "autumnmooncat.wildfrost.spirefrost.spikeslime2",
                            changeToCardName = "autumnmooncat.wildfrost.spirefrost.spikeslime3"
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
