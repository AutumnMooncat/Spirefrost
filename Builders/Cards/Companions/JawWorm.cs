using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;

namespace Spirefrost.Builders.Cards.Companions
{
    internal class JawWorm : SpirefrostBuilder
    {
        internal static string ID => "jawworm";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Jaw Worm")
                .SetSprites("Units/JawWorm.png", "Units/JawWormBG.png")
                .SetStats(4, 3, 3)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(WhenHealedReduceCounter.ID, 1)
                    };
                });
        }
    }
}
