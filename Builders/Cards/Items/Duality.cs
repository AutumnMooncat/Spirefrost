using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.WatcherItems)]
    internal class Duality : SpirefrostBuilder
    {
        internal static string ID => "duality";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Duality")
                .SetSprites("Items/Duality.png", "Items/DualityBG.png")
                .WithValue(55)
                .CanPlayOnHand(true)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Shell", 2),
                        SStack("Spice", 2),
                        SStack(InstantEqualizeShellSpice.ID, 1)
                    };
                });
        }
    }
}
