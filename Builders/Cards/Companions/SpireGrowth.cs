using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.IroncladUnits)]
    internal class SpireGrowth : SpirefrostBuilder
    {
        internal static string ID => "spiregrowth";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Spire Growth")
                .SetSprites("Units/SpireGrowth.png", "Units/SpireGrowthBG.png")
                .SetStats(8, 2, 3)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(WhileActiveConstrictedToFrontEnemy.ID, 1)
                    };
                });
        }
    }
}
