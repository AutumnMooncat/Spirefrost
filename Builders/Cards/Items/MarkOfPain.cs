using Deadpan.Enums.Engine.Components.Modding;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.IroncladItems)]
    internal class MarkOfPain : SpirefrostBuilder
    {
        internal static string ID => "markofpain";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Mark of Pain")
                .SetSprites("Items/MarkOfPain.png", "Items/MarkOfPainBG.png")
                .WithValue(55)
                .SetTraits(TStack("Consume", 1))
                .SetAttackEffect(SStack("Reduce Max Counter", 1), SStack("Reduce Max Health", 3));
        }
    }
}
