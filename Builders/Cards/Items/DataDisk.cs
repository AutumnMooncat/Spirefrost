using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.DefectItems)]
    internal class DataDisk : SpirefrostBuilder
    {
        internal static string ID => "datadisk";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Data Disk")
                .SetSprites("Items/DataDisk.png", "Items/DataDiskBG.png")
                .WithValue(55)
                .CanPlayOnHand(true)
                .SetTraits(TStack("Consume", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(InstantApplyTemporaryFocus.ID, 1)
                    };
                });
        }
    }
}
