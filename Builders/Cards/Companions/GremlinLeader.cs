using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;

namespace Spirefrost.Builders.Cards.Companions
{
    internal class GremlinLeader : SpirefrostBuilder
    {
        internal static string ID => "gremlinleader";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Gremlin Leader")
                .SetSprites("Units/GremlinLeader.png", "Units/GremlinLeaderBG.png")
                .SetStats(9, 0, 5)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("MultiHit", 2),
                        SStack(WhileActiveIncreaseAttackToGremlins.ID, 2)
                    };
                });
        }
    }
}
