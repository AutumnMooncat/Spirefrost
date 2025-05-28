using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.Units)]
    internal class Mystic : SpirefrostBuilder
    {
        internal static string ID => "mystic";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Mystic")
                .SetSprites("Units/Mystic.png", "Units/MysticBG.png")
                .SetStats(4, null, 4)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Turn Add Health & Attack To Allies", 1),
                    };
                });
        }
    }
}
