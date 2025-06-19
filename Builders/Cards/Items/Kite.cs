using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.SilentItems)]
    internal class Kite : SpirefrostBuilder
    {
        internal static string ID => "kite";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Hovering Kite")
                .SetSprites("Items/Kite.png", "Items/KiteBG.png")
                .WithValue(60)
                .CanPlayOnHand(true)
                .SetTraits(TStack("Consume", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Reduce Max Counter", 1),
                        SStack("Instant Gain Aimless", 1)
                    };
                });
        }
    }
}
