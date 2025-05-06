using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects.IconEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.Items)]
    internal class WingBoots : SpirefrostBuilder
    {
        internal static string ID => "wingboots";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Wing Boots")
                .SetSprites("Items/WingBoots.png", "Items/WingBootsBG.png")
                .WithValue(45)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(Flight.ID, 1)
                    };
                });
        }
    }
}
