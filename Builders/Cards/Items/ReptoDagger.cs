using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.Cards.Items
{
    internal class ReptoDagger : SpirefrostBuilder
    {
        internal static string ID => "reptodagger";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Dagger")
                .SetSprites("Items/ReptoDagger.png", "Items/ReptoDaggerBG.png")
                .WithValue(50)
                .SetDamage(4)
                .WithFlavour("Poke Poke :)");
        }
    }
}
