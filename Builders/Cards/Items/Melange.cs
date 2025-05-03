using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.WatcherItems)]
    internal class Melange : SpirefrostBuilder
    {
        internal static string ID => "melange";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Melange")
                .SetSprites("Items/Melange.png", "Items/MelangeBG.png")
                .WithValue(50)
                .CanPlayOnHand(true)
                .SetTraits(TStack("Noomlin", 1))
                .WithFlavour("Must flow")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Spice", 3)
                    };
                });
        }
    }
}
