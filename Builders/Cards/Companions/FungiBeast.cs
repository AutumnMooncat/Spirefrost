using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.SilentUnits)]
    internal class FungiBeast : SpirefrostBuilder
    {
        internal static string ID => "fungi";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Fungi Beast")
                .SetSprites("Units/Fungi.png", "Units/FungiBG.png")
                .SetStats(4, 0, 3)
                .WithValue(50)
                .WithEyes(FullID, 
                (1.175f, 1f, 0.7f, 0.7f, -10f)/*,
                (0.175f, 1.75f, 0.7f, 0.7f, -5f),
                (0.5f, 1.725f, 0.65f, 0.65f, -5f)*/)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[] 
                    {
                        SStack("Shroom", 3)
                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(WhenHitApplyShroomToFrontEnemies.ID, 2)
                    };
                });
        }
    }
}
