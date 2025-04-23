using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.Cards.Items
{
    [SpirefrostUtils.AutoAdd.Ignore]
    internal class BlueCandle : SpirefrostBuilder
    {
        internal static string ID => "bluecandle";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Blue Candle")
                .SetSprites("Items/BlueCandle.png", "Items/BlueCandleBG.png")
                .WithValue(60)
                .SetTraits(TStack("Consume", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Ritual", 1),
                        SStack("Reduce Max Health", 3)
                    };
                });
        }
    }
}
