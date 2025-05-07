using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Traits;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.WatcherStarterItems, 1, 1)]
    internal class PureWater : SpirefrostBuilder
    {
        internal static string ID => "purewater";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Pure Water")
                .SetSprites("Items/PureWater.png", "Items/PureWaterBG.png")
                .WithValue(25)
                .SetAttackEffect(SStack("Reduce Counter", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.traits = new System.Collections.Generic.List<CardData.TraitStacks>
                    {
                        TStack(RetainTrait.ID, 1),
                        TStack(PatientTrait.ID, 1)
                    };
                });
        }
    }
}
