using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Traits;

namespace Spirefrost.Builders.Cards.Items
{
    internal class HolyWater : SpirefrostBuilder
    {
        internal static string ID => "holywater";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Holy Water")
                .SetSprites("Items/HolyWater.png", "Items/HolyWaterBG.png")
                .WithValue(50)
                .SetAttackEffect(SStack("Reduce Counter", 3))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.traits = new System.Collections.Generic.List<CardData.TraitStacks>
                    {
                        TStack(RetainTrait.ID, 1),
                        TStack("Consume", 1)
                    };
                });
        }
    }
}
