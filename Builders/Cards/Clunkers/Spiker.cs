using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.Cards.Clunkers
{
    internal class Spiker : SpirefrostBuilder
    {
        internal static string ID => "spiker";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Spiker")
                .SetSprites("Units/Spiker.png", "Units/SpikerBG.png")
                .SetStats(null, null, 3)
                .WithCardType("Clunker")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Scrap", 1),
                        SStack("Teeth", 2),
                        SStack("On Turn Apply Teeth To Self", 2)
                    };
                });
        }
    }
}
