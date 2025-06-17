using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.Items)]
    internal class BustedCrown : SpirefrostBuilder
    {
        internal static string ID => "bustedcrown";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Busted Crown")
                .SetSprites("Items/BustedCrown.png", "Items/BustedCrownBG.png")
                .WithValue(60)
                .SetDamage(0)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(BonusDamageForEachCrownedCard.ID, 2)
                    };
                });
        }
    }
}
