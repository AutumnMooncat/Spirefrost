using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;

namespace Spirefrost.Builders.Cards.Summons
{
    internal class PlasmaOrb : SpirefrostBuilder
    {
        internal static string ID => "plasmaorb";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Plasma")
                .SetSprites("Summons/PlasmaOrb.png", "Summons/PlasmaOrbBG.png")
                .SetStats(2, null, 1)
                .WithCardType("Summoned")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(OnCardPlayedReduceCounterToRandomAlly.ID, 1)
                    };
                });
        }
    }
}
