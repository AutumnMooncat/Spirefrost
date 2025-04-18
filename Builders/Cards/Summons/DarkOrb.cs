using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;

namespace Spirefrost.Builders.Cards.Summons
{
    internal class DarkOrb : SpirefrostBuilder
    {
        internal static string ID => "darkorb";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Dark")
                .SetSprites("Summons/DarkOrb.png", "Summons/DarkOrbBG.png")
                .SetStats(2, null, 1)
                .WithCardType("Summoned")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(OnTurnApplyExplodeToSelf.ID, 2)
                    };
                });
        }
    }
}
