using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using Spirefrost.Builders.StatusEffects.IconEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.Items)]
    internal class GremlinVisage : SpirefrostBuilder
    {
        internal static string ID => "gremlinvisage";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Gremlin Visage")
                .SetSprites("Items/GremlinVisage.png", "Items/GremlinVisageBG.png")
                .WithValue(55)
                .SetDamage(2)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Frost", 1),
                        SStack(DoubleFrost.ID, 1),
                    };
                });
        }
    }
}
