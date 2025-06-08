using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.WatcherUnits)]
    internal class Byrd : SpirefrostBuilder
    {
        internal static string ID => "byrd";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Byrd")
                .SetSprites("Units/Byrd.png", "Units/ByrdBG.png")
                .SetStats(4, 1, 4)
                .WithValue(50)
                .WithEyes(FullID, (1.0f, 0.95f, 0.85f, 0.85f, -5f))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(WhenDeployedApplyFlightToSelf.ID, 3),
                        SStack("MultiHit", 2)
                    };
                });
        }
    }
}
