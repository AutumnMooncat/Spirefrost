using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.DefectItems)]
    internal class Capacitor : SpirefrostBuilder
    {
        internal static string ID => "capacitor";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Runic Capacitor")
                .SetSprites("Items/Capacitor.png", "Items/CapacitorBG.png")
                .WithValue(60)
                .CanPlayOnHand(true)
                .SetTraits(TStack("Consume", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Increase Max Counter", 1),
                        SStack(InstantGainBarrage.ID, 1)
                    };
                });
        }
    }
}
