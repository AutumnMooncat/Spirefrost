using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Clunkers
{
    [ToPoolList(PoolListType.Items)]
    internal class Repulser : SpirefrostBuilder
    {
        internal static string ID => "repulser";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Repulser")
                .SetSprites("Units/Repulser.png", "Units/RepulserBG.png")
                .SetStats(null, null, 0)
                .WithCardType("Clunker")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Scrap", 1),
                        SStack(WhileActiveIncreaseEffectsToAlliesAndEnemiesNoBoosting.ID, 1)
                    };
                });
        }
    }
}
