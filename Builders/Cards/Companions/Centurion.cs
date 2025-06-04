using Deadpan.Enums.Engine.Components.Modding;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.Units)]
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
                .WithEyes(FullID, (0.075f, 1.375f, 1.5f, 0.75f, -12.5f))
                .SetStartWithEffect(SStack("On Turn Apply Shell To AllyBehind", 3));
        }
    }
}
