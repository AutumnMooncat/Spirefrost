using Deadpan.Enums.Engine.Components.Modding;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.SilentUnits)]
    internal class SnakePlant : SpirefrostBuilder
    {
        internal static string ID => "snakeplant";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Snake Plant")
                .SetSprites("Units/SnakePlant.png", "Units/SnakePlantBG.png")
                .SetStats(8, 1, 5)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("When Hit Apply Shell To Self", 2),
                        SStack("MultiHit", 2)
                    };
                });
        }
    }
}
