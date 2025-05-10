using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.IroncladUnits)]
    internal class Reptomancer : SpirefrostBuilder
    {
        internal static string ID => "reptomancer";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Repromancer")
                .SetSprites("Units/Repromancer.png", "Units/RepromancerBG.png")
                .SetStats(8, 4, 0)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(WhenDeployedAddReptoDaggerToHand.ID, 1),
                        SStack(TriggerWhenReptoDaggerPlayed.ID, 1)
                    };
                });
        }
    }
}
