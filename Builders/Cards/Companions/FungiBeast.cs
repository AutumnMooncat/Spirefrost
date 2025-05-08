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
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[] 
                    {
                        SStack("Shroom", 3)
                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(WhenHitApplyShroomToEnemies.ID, 1)
                    };
                });
        }
    }
}
