using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.Cards.Companions
{
    internal class Centurion : SpirefrostBuilder
    {
        internal static string ID => "centurion";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Centurion")
                .SetSprites("Units/Centurion.png", "Units/CenturionBG.png")
                .SetStats(10, 3, 4)
                .WithValue(50)
                .SetTraits(TStack("Frontline", 1))
                .SetStartWithEffect(SStack("On Turn Apply Shell To Allies", 1));
        }
    }
}
