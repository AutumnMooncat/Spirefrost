using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.WatcherItems)]
    internal class GoldenEye : SpirefrostBuilder
    {
        internal static string ID => "goldeneye";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Golden Eye")
                .SetSprites("Items/GoldenEye.png", "Items/GoldenEyeBG.png")
                .WithValue(45)
                .CanPlayOnHand(true)
                .NeedsTarget(false)
                .SetTraits(TStack("Noomlin", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(OnCardPlayedDoDiscoveryGoldenEye.ID, 1)
                    };
                });
        }
    }
}
