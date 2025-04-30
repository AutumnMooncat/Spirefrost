using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.IroncladItems)]
    internal class Ashes : SpirefrostBuilder
    {
        internal static string ID => "ashes";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Charon's Ashes")
                .SetSprites("Items/Ashes.png", "Items/AshesBG.png")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Sacrifice Card In Hand", 1),
                        SStack(InstantDealCurrentAttackToEnemies.ID, 1)
                    };
                });
        }
    }
}
