using Deadpan.Enums.Engine.Components.Modding;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.Items)]
    internal class ThreadNeedle : SpirefrostBuilder
    {
        internal static string ID => "threadneedle";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Thread and Needle")
                .SetSprites("Items/ThreadNeedle.png", "Items/ThreadNeedleBG.png")
                .WithValue(50)
                .CanPlayOnHand(true)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Instant Add Scrap", 2)
                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Card Played Reduce Attack Effect 1 To Self", 1)
                    };
                });
        }
    }
}
