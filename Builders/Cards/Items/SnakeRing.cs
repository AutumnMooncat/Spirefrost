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
                .CreateItem(ID, "Ring of the Snake")
                .SetSprites("Items/SnakeRing.png", "Items/SnakeRingBG.png")
                .WithValue(25)
                .SetDamage(0)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Shroom", 2)
                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Kill Draw", 2)
                    };
                });
        }
    }
}
