using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Clunkers
{
    [ToPoolList(PoolListType.StarterItems,1, 4)]
    internal class BronzeOrb : SpirefrostBuilder
    {
        internal static string ID => "bronzeorb";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Bronze Orb")
                .SetSprites("Units/BronzeOrb.png", "Units/BronzeOrbBG.png")
                .SetStats(null, null, 0)
                .WithCardType("Clunker")
                .WithValue(25)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Scrap", 1),
                        SStack(WhenDestroyedTriggerToAllyBehind.ID, 1)
                    };
                });
        }
    }
}
