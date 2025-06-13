using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.Units)]
    internal class GremlinLeader : SpirefrostBuilder
    {
        internal static string ID => "gremlinleader";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Gremlin Leader")
                .SetSprites("Units/GremlinLeader.png", "Units/GremlinLeaderBG.png")
                .SetStats(7, 0, 5)
                .WithValue(50)
                .WithEyes(FullID, 
                (0.025f, 1.4f, 0.8f, 0.8f, -10f),
                (0.425f, 1.325f, 0.7f, 0.7f, -10f))
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
