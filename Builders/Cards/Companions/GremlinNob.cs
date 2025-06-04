using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using Spirefrost.Builders.StatusEffects.IconEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.Units)]
    internal class GremlinNob : SpirefrostBuilder
    {
        internal static string ID => "gremlinnob";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Gremlin Nob")
                .SetSprites("Units/Nob.png", "Units/NobBG.png")
                .SetStats(12, 2, 5)
                .WithValue(50)
                .WithEyes(FullID, 
                (0.325f, 1.425f, 0.35f, 0.35f, -10f),
                (0.55f, 1.375f, 0.35f, 0.35f, -10f),
                (0.225f, 1.525f, 0.7f, 0.7f, -10f),
                (0.675f, 1.45f, 0.7f, 0.7f, -10f))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    MainModFile.instance.maskedSpries[data.name] = MainModFile.instance.ImagePath("Units/NobMask.png").ToSprite();
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Demonize", 2)
                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(WhenSkillItemPlayedGainAttack.ID, 1)
                    };
                });
        }
    }
}
