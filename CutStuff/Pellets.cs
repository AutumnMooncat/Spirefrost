using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.Items)]
    internal class Pellets : SpirefrostBuilder
    {
        internal static string ID => "pellets";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Orange Pellets")
                .SetSprites("Items/Pellets.png", "Items/PelletsBG.png")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Heal", 4),
                        SStack(InstantCleanseAndApplyAttackPerRemoved.ID, 2)
                    };
                });
        }
    }
}
