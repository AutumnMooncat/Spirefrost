using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects.IconEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.Items)]
    internal class RedMask : SpirefrostBuilder
    {
        internal static string ID => "redmask";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Red Mask")
                .SetSprites("Items/RedMask.png", "Items/RedMaskBG.png")
                .WithValue(55)
                .SetDamage(0)
                .SetTraits(TStack("Barrage", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(Weak.ID, 1)
                    };
                });
        }
    }
}
