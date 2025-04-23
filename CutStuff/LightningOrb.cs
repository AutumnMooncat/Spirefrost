using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.Cards.Summons
{
    internal class LightningOrb : SpirefrostBuilder
    {
        internal static string ID => "lightningorb";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Lightning")
                .SetSprites("Summons/LightningOrb.png", "Summons/LightningOrbBG.png")
                .SetStats(2, 3, 1)
                .WithCardType("Summoned")
                .SetTraits(TStack("Aimless", 1));
        }
    }
}
