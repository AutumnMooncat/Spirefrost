using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;

namespace Spirefrost.Builders.Cards.Companions
{
    internal class Looter : SpirefrostBuilder
    {
        internal static string ID => "looter";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Looter")
                .SetSprites("Units/Looter.png", "Units/LooterBG.png")
                .SetStats(5, 4, 3)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Kill Apply Gold To Self", 5),
                        SStack(OnKillTriggerAgain.ID, 1)
                    };
                });
        }
    }
}
