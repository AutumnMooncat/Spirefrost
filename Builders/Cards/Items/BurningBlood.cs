using Deadpan.Enums.Engine.Components.Modding;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.IroncladStarterItems, 1, 1)]
    internal class BurningBlood : SpirefrostBuilder
    {
        internal static string ID => "burningblood";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Burning Blood")
                .SetSprites("Items/BurningBlood.png", "Items/BurningBloodBG.png")
                .WithValue(25)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Heal", 3)
                    };
                });
        }
    }
}
