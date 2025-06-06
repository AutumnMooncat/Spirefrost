using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.Units)]
    internal class Lagavulin : SpirefrostBuilder
    {
        internal static string ID => "lagavulin";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Lagavulin")
                .SetSprites("Units/Lagavulin.png", "Units/LagavulinBG.png")
                .SetStats(8, 4, 0)
                .WithValue(50)
                .SetTraits(TStack("Smackback", 1))
                .WithEyes(FullID, (0.45f, -0.05f, 0.85f, 0.85f, 0f))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    MainModFile.instance.maskedSpries[data.name] = MainModFile.instance.ImagePath("Units/LagavulinMask.png").ToSprite();
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Shell", 4),
                        SStack(OnHitEqualDamageToFrostedTarget.ID, 1)
                        //SStack(WhenHitByFrostedCardGainFrenzy.ID, 1)
                    };
                });
        }
    }
}
