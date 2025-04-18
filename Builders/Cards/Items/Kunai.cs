using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.Items)]
    internal class Kunai : SpirefrostBuilder
    {
        internal static string ID => "kunai";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Kunai")
                .SetSprites("Items/Kunai.png", "Items/KunaiBG.png")
                .WithValue(50)
                .SetDamage(4)
                .WithFlavour("I am the era.")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(AttackAboveXCountsAsFrenzy.ID, 1)
                    };
                });
        }
    }
}
