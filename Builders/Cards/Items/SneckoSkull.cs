using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.SilentItems)]
    internal class SneckoSkull : SpirefrostBuilder
    {
        internal static string ID => "sneckoskull";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Snecko Skull")
                .SetSprites("Items/SneckoSkull.png", "Items/SneckoSkullBG.png")
                .WithValue(50)
                .SetDamage(0)
                .SetAttackEffect(SStack("Shroom", 2))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(WhileInHandIncreaseAllShroomApplied.ID, 1)
                    };
                });
        }
    }
}
