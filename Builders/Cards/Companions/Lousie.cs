using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects.IconEffects;

namespace Spirefrost.Builders.Cards.Companions
{
    internal class Lousie : SpirefrostBuilder
    {
        internal static string ID => "louse";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Lousie")
                .SetSprites("Units/Louse.png", "Units/LouseBG.png")
                .SetStats(4, 3, 4)
                .WithValue(25)
                .IsPet((ChallengeData)null, true)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Frost", 2)
                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(RollUp.ID, 2)
                    };
                });
        }
    }
}
