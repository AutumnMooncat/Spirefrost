using Deadpan.Enums.Engine.Components.Modding;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.Items)]
    internal class GremlinHorn : SpirefrostBuilder
    {
        internal static string ID => "gremlinhorn";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Gremlin Horn")
                .SetSprites("Items/GremlinHorn.png", "Items/GremlinHornBG.png")
                .WithValue(50)
                .SetDamage(5)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Kill Draw", 2)
                    };
                });
        }
    }
}
