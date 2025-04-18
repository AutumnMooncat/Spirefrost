using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.Cards.Companions
{
    internal class MadGremlin : SpirefrostBuilder
    {
        internal static string ID => "madgremlin";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Mad Gremlin")
                .SetSprites("Units/MadGremlin.png", "Units/MadGremlinBG.png")
                .SetStats(5, 2, 4)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("When Hit Gain Attack To Self (No Ping)", 1)
                    };
                });
        }
    }
}
