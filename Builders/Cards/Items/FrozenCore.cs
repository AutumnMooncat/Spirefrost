using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects.IconEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.DefectItems)]
    internal class FrozenCore : SpirefrostBuilder
    {
        internal static string ID => "frozencore";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Frozen Core")
                .SetSprites("Items/FrozenCore.png", "Items/FrozenCoreBG.png")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(FrostOrb.ID, FrostOrb.ApplyAmount)
                    };
                });
        }
    }
}
