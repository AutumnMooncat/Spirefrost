using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using Spirefrost.Builders.StatusEffects.IconEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.Units)]
    internal class Nob : SpirefrostBuilder
    {
        internal static string ID => "nob";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Gremlin Nob")
                .SetSprites("Units/Nob.png", "Units/NobBG.png")
                .SetStats(12, 2, 5)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    MainModFile.instance.maskedSpries[data.name] = MainModFile.instance.ImagePath("Units/NobMask.png").ToSprite();
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(Vulnerable.ID, 2)
                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(WhenSkillItemPlayedGainAttack.ID, 1)
                    };
                });
        }
    }
}
