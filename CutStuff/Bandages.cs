using Deadpan.Enums.Engine.Components.Modding;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.Items)]
    internal class Bandages : SpirefrostBuilder
    {
        internal static string ID => "bandages";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Tough Bandages")
                .SetSprites("Items/Bandages.png", "Items/BandagesBG.png")
                .WithValue(40)
                .SetAttackEffect(SStack("Increase Max Health", 1), SStack("Shell", 3));
        }
    }
}
