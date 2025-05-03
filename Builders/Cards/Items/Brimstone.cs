using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.IroncladItems)]
    internal class Brimstone : SpirefrostBuilder
    {
        internal static string ID => "brimstone";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Brimstone")
                .SetSprites("Items/Brimstone.png", "Items/BrimstoneBG.png")
                .WithValue(50)
                .CanPlayOnHand(true)
                .NeedsTarget(false)
                .SetTraits(TStack("Consume", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(OnCardPlayedAddAttackToAllies.ID, 2),
                        SStack(OnCardPlayedAddAttackToEnemies.ID, 1)
                    };
                });
        }
    }
}
