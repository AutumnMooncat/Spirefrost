using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using Spirefrost.Builders.StatusEffects.IconEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.Items)]
    internal class PocketWatch : SpirefrostBuilder
    {
        internal static string ID => "pocketwatch";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Pocketwatch")
                .SetSprites("Items/Pocketwatch.png", "Items/PocketwatchBG.png")
                .WithValue(40)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(DoubleTap.ID, 1),
                        SStack(IncreaseCounter.ID, 1)
                    };
                });
        }
    }
}
