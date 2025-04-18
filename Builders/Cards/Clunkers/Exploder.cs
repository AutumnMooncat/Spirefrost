using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;

namespace Spirefrost.Builders.Cards.Clunkers
{
    internal class Exploder : SpirefrostBuilder
    {
        internal static string ID => "exploder";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Exploder")
                .SetSprites("Units/Exploder.png", "Units/ExploderBG.png")
                .SetStats(null, null, 0)
                .WithCardType("Clunker")
                .WithValue(50)
                .SetTraits(TStack("Explode", 4))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Scrap", 1),
                        SStack(WhileActiveExplodeToAllies.ID, 4)
                    };
                });
        }
    }
}
