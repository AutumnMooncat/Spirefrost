using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.WatcherItems)]
    internal class VioletLotus : SpirefrostBuilder
    {
        internal static string ID => "violetlotus";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Violet Lotus")
                .SetSprites("Items/VioletLotus.png", "Items/VioletLotusBG.png")
                .WithValue(60)
                .SetTraits(TStack("Consume", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(IncreaseCounter.ID, 3),
                        SStack("Reduce Max Counter", 1)
                    };
                });
        }
    }
}
