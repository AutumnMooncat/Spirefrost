using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;

namespace Spirefrost.Builders.Cards.Companions
{
    internal class Chosen : SpirefrostBuilder
    {
        internal static string ID => "chosen";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Chosen")
                .SetSprites("Units/Chosen.png", "Units/ChosenBG.png")
                .SetStats(11, 1, 3)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("MultiHit", 1),
                        SStack(OnTurnApplyAmplifyToAllyBehind.ID, 1)
                    };
                });
        }
    }
}
