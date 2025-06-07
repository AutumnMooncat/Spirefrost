using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.WatcherUnits)]
    internal class Maw : SpirefrostBuilder
    {
        internal static string ID => "maw";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "The Maw")
                .SetSprites("Units/Maw.png", "Units/MawBG.png")
                .SetStats(8, 2, 5)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(OnKillGainTargetAttack.ID, 1)
                    };
                });
        }
    }
}
