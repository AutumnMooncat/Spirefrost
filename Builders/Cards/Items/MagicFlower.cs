using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.IroncladItems)]
    internal class MagicFlower : SpirefrostBuilder
    {
        internal static string ID => "magicflower";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Magic Flower")
                .SetSprites("Items/MagicFlower.png", "Items/MagicFlowerBG.png")
                .WithValue(60)
                .CanPlayOnHand(true)
                .SetTraits(TStack("Consume", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(InstantGainFlourish.ID, 1)
                    };
                });
        }
    }
}
