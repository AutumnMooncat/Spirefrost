using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.Items)]
    internal class Fan : SpirefrostBuilder
    {
        internal static string ID => "fan";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Ornamental Fan")
                .SetSprites("Items/Fan.png", "Items/FanBG.png")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Shell", 2)
                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(WhenAttackItemPlayedTemporaryIncreaseEffects.ID, 1)
                    };
                });
        }
    }
}
