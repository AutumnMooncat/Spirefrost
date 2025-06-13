using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.WatcherUnits)]
    internal class ShieldGremlin : SpirefrostBuilder
    {
        internal static string ID => "shieldgremlin";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Shield Gremlin")
                .SetSprites("Units/ShieldGremlin.png", "Units/ShieldGremlinBG.png")
                .SetStats(3, 3, 3)
                .WithValue(50)
                .WithEyes(FullID, 
                (0.15f, 2.35f, 0.45f, 0.45f, 0f),
                (-0.05f, 2.35f, 0.5f, 0.5f, 0f))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Turn Apply Shell To Self", 2),
                        SStack(BonusDamageEqualToSpiceCustom.ID, 1)
                    };
                });
        }
    }
}
