using Deadpan.Enums.Engine.Components.Modding;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.Units)]
    internal class Snecko : SpirefrostBuilder
    {
        internal static string ID => "snecko";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Snecko")
                .SetSprites("Units/Snecko.png", "Units/SneckoBG.png")
                .SetStats(7, 0, 5)
                .WithValue(50)
                .SetTraits(TStack("Aimless", 1))
                .WithEyes(FullID, (0.45f, 1.15f, 1f, 1f, -10f))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Haze", 1)
                    };
                });
        }
    }
}
