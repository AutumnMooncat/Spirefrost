using Deadpan.Enums.Engine.Components.Modding;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Clunkers
{
    [ToPoolList(PoolListType.Items)]
    internal class SpireShield : SpirefrostBuilder
    {
        internal static string ID => "spireshield";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Spire Shield")
                .SetSprites("Units/SpireShield.png", "Units/SpireShieldBG.png")
                .SetStats(null, 1, 0)
                .WithCardType("Clunker")
                .WithValue(50)
                .SetTraits(TStack("Smackback", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Scrap", 3),
                        SStack("Bonus Damage Equal To Scrap On Board", 1)
                    };
                });
        }
    }
}
