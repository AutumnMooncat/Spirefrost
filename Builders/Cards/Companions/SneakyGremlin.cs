using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.Cards.Companions
{
    internal class SneakyGremlin : SpirefrostBuilder
    {
        internal static string ID => "sneakygremlin";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Sneaky Gremlin")
                .SetSprites("Units/SneakyGremlin.png", "Units/SneakyGremlinBG.png")
                .SetStats(2, 2, 0)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Trigger Against When Ally Attacks", 1)
                    };
                });
        }
    }
}
