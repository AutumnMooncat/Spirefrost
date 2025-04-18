using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.Cards.Items
{
    internal class Shiv : SpirefrostBuilder
    {
        internal static string ID => "shiv";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Shiv")
                .SetSprites("Items/Shiv.png", "Items/ShivBG.png")
                .WithValue(50)
                .SetDamage(2)
                .SetTraits(TStack("Zoomlin", 1), TStack("Consume", 1));
        }
    }
}
