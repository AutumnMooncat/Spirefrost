using Deadpan.Enums.Engine.Components.Modding;

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
                .SetTraits(TStack("Consume", 1));
        }
    }
}
