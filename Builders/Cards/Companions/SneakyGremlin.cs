using Deadpan.Enums.Engine.Components.Modding;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.SilentUnits)]
    internal class SneakyGremlin : SpirefrostBuilder
    {
        internal static string ID => "sneakygremlin";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Sneaky Gremlin")
                .SetSprites("Units/SneakyGremlin.png", "Units/SneakyGremlinBG.png")
                .SetStats(2, 1, 0)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Shroom", 1)
                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Trigger When Ally Attacks", 1)
                    };
                });
        }
    }
}
