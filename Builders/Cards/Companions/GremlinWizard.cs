using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.Cards.Companions
{
    internal class GremlinWizard : SpirefrostBuilder
    {
        internal static string ID => "gremlinwizard";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Gremlin Wizard")
                .SetSprites("Units/GremlinWizard.png", "Units/GremlinWizardBG.png")
                .SetStats(5, 6, 6)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("When Hit Reduce Counter To Self", 1)
                    };
                });
        }
    }
}
