using Deadpan.Enums.Engine.Components.Modding;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.IroncladUnits)]
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
                .SetTraits(TStack("Fury", 2))
                .WithEyes(FullID, 
                (-0.15f, 1.775f, 0.6f, 0.6f, 0f),
                (0.15f, 1.775f, 0.55f, 0.55f, 0f))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("When Hit Gain Attack To Self (No Ping)", 2)
                    };
                });
        }
    }
}
