using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.Items)]
    internal class Battery : SpirefrostBuilder
    {
        internal static string ID => "battery";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Nuclear Battery")
                .SetSprites("Items/Battery.png", "Items/BatteryBG.png")
                .WithValue(60)
                .SetTraits(TStack("Consume", 1))
                .CanPlayOnHand(true)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(IncreaseEffectsWithDesc.ID, 1)
                    };
                });
        }
    }
}
