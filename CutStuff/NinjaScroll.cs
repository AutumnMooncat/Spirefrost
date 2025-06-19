using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.SilentItems)]
    internal class NinjaScroll : SpirefrostBuilder
    {
        internal static string ID => "ninjascroll";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Ninja Scroll")
                .SetSprites("Items/NinjaScroll.png", "Items/NinjaScrollBG.png")
                .WithValue(50)
                .CanPlayOnHand(true)
                .NeedsTarget(false)
                .SetTraits(TStack("Noomlin", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(OnCardPlayedAddShivToHand.ID, 2)
                    };
                });
        }
    }
}
