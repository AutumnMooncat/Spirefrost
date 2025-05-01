using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.IroncladItems)]
    internal class RedSkull : SpirefrostBuilder
    {
        internal static string ID => "redskull";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Red Skull")
                .SetSprites("Items/RedSkull.png", "Items/RedSkullBG.png")
                .WithValue(60)
                .SetTraits(TStack("Consume", 1))
                .CanPlayOnHand(true)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(HalveHealthApplyAttackEffect.ID, 1)
                    };
                });
        }
    }
}
