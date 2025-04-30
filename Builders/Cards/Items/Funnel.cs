using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.SilentItems)]
    internal class Funnel : SpirefrostBuilder
    {
        internal static string ID => "funnel";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Twisted Funnel")
                .SetSprites("Items/Funnel.png", "Items/FunnelBG.png")
                .WithValue(50)
                .SetDamage(2)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(OnHitEqualShroomToTarget.ID, 1)
                    };
                });
        }
    }
}
