using Deadpan.Enums.Engine.Components.Modding;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.StarterItems, 4, 0)]
    internal class Shuriken : SpirefrostBuilder
    {
        internal static string ID => "shuriken";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Shuriken")
                .SetSprites("Items/Shuriken.png", "Items/ShurikenBG.png")
                .WithValue(10)
                .SetDamage(2)
                .SetStartWithEffect(SStack("On Hit Damage Damaged Target", 1));
        }
    }
}
