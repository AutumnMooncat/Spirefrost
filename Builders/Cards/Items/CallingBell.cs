using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.Items)]
    internal class CallingBell : SpirefrostBuilder
    {
        internal static string ID => "callingbell";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Calling Bell")
                .SetSprites("Items/CallingBell.png", "Items/CallingBellBG.png")
                .WithValue(55)
                .CanPlayOnHand(true)
                .SetTraits(TStack("Consume", 1))
                .WithFlavour("Bing Bong")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(ReduceAttackWithText.ID, 2)
                    };
                });
        }
    }
}
