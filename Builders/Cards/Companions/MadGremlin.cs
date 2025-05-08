using Deadpan.Enums.Engine.Components.Modding;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.WatcherUnits)]
    internal class MadGremlin : SpirefrostBuilder
    {
        internal static string ID => "madgremlin";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Mad Gremlin")
                .SetSprites("Units/MadGremlin.png", "Units/MadGremlinBG.png")
                .SetStats(5, 1, 4)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("When Hit Apply Spice To Self", 1),
                        SStack("Halt Spice With Text", 1)
                    };
                });
        }
    }
}
