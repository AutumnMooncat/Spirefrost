using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.Items)]
    internal class Boot : SpirefrostBuilder
    {
        internal static string ID => "boot";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Boot")
                .SetSprites("Items/Boot.png", "Items/BootBG.png")
                .WithValue(55)
                .SetTraits(TStack("Consume", 1))
                .CanPlayOnHand(true)
                .CanPlayOnBoard(false)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(SetAttackOfCardInHand.ID, 5)
                    };
                });
        }
    }
}
