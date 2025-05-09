using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Clunkers
{
    [ToPoolList(PoolListType.Items)]
    internal class OrbWalker : SpirefrostBuilder
    {
        internal static string ID => "orbwalker";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Orb Walker")
                .SetSprites("Units/OrbWalker.png", "Units/OrbWalkerBG.png")
                .SetStats(null, null, 0)
                .WithCardType("Clunker")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Scrap", 1),
                        SStack(WhileActiveRitualToAllies.ID, 1)
                    };
                });
        }
    }
}
