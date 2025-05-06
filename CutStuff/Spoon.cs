using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.Items)]
    internal class Spoon : SpirefrostBuilder
    {
        internal static string ID => "spoon";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Strange Spoon")
                .SetSprites("Items/Spoon.png", "Items/SpoonBG.png")
                .WithValue(50)
                .CanPlayOnBoard(false)
                .CanPlayOnEnemy(false)
                .CanPlayOnFriendly(false)
                .CanPlayOnHand(false)
                .WithPlayType(Card.PlayType.None)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(WhileInHandRedirectConsume.ID, 1)
                    };
                });
        }
    }
}
