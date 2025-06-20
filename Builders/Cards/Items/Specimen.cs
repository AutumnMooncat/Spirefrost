using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.SilentItems)]
    internal class Specimen : SpirefrostBuilder
    {
        internal static string ID => "specimen";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "The Specimen")
                .SetSprites("Items/Specimen.png", "Items/SpecimenBG.png")
                .WithValue(50)
                .SetDamage(0)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Shroom", 2),
                        SStack(InstantApplyCurrentShroomToRandomEnemy.ID, 1)
                    };
                });
        }
    }
}
