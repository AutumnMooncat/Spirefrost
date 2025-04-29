using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.SilentStarterItems, 1, 1)]
    internal class SnakeRing : SpirefrostBuilder
    {
        internal static string ID => "snakering";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Snake Ring")
                .SetSprites("Items/SnakeRing.png", "Items/SnakeRingBG.png")
                .WithValue(25)
                .CanPlayOnBoard(false)
                .CanPlayOnEnemy(false)
                .CanPlayOnFriendly(false)
                .CanPlayOnHand(false)
                .WithPlayType(Card.PlayType.None)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(WhenDrawnDraw.ID, 2)
                    };
                });
        }
    }
}
