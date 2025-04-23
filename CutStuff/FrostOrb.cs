using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.Cards.Summons
{
    internal class FrostOrb : SpirefrostBuilder
    {
        internal static string ID => "frostorb";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Frost")
                .SetSprites("Summons/FrostOrb.png", "Summons/FrostOrbBG.png")
                .SetStats(2, 0, 1)
                .WithCardType("Summoned")
                .SetAttackEffect(SStack("Frost", 1));
        }
    }
}
