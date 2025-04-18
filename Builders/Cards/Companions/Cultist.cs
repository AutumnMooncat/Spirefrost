using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects.IconEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.Units)]
    internal class Cultist : SpirefrostBuilder
    {
        internal static string ID => "cultist";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Cultist")
                .SetSprites("Units/Cultist.png", "Units/CultistBG.png")
                .SetStats(6, 2, 5)
                .WithValue(50)
                .WithFlavour("Caw Caw!")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(Ritual.ID, 1)
                    };
                });
        }
    }
}
