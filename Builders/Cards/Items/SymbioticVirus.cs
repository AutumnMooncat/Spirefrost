using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects.IconEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.DefectItems)]
    internal class SymbioticVirus : SpirefrostBuilder
    {
        internal static string ID => "symbioticvirus";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Symbiotic Virus")
                .SetSprites("Items/SymbioticVirus.png", "Items/SymbioticVirusBG.png")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(DarkOrb.ID, DarkOrb.ApplyAmount)
                    };
                });
        }
    }
}
