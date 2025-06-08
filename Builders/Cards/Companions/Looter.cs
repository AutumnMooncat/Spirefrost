using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using Spirefrost.Builders.StatusEffects.IconEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.Units)]
    internal class Looter : SpirefrostBuilder
    {
        internal static string ID => "looter";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Looter & Mugger")
                .SetSprites("Units/LooterMugger.png", "Units/LooterBG.png")
                .SetStats(5, 2, 3)
                .WithValue(50)
                .WithEyes(FullID, 
                (-0.25f, 1.15f, 0.5f, 0.5f, -20f),
                (0.625f, 1.325f, 0.45f, 0.45f, -20f))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Kill Apply Gold To Self", 5),
                        SStack(ExtraCounter.ID, 4),
                        //SStack(OnKillTriggerAgain.ID, 1)
                    };
                });
        }
    }
}
