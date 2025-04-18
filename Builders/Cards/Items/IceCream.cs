using Deadpan.Enums.Engine.Components.Modding;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.Items)]
    internal class IceCream : SpirefrostBuilder
    {
        internal static string ID => "icecream";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Ice Cream")
                .SetSprites("Items/IceCream.png", "Items/IceCreamBG.png")
                .WithValue(60)
                .SetDamage(0)
                .SetTraits(TStack("Consume", 1))
                .SetAttackEffect(SStack("Reduce Max Counter", 1), SStack("Snow", 2));
        }
    }
}
