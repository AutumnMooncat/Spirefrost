using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.IroncladUnits)]
    internal class ShelledParasite : SpirefrostBuilder
    {
        internal static string ID => "shelledparasite";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Shelled Parasite")
                .SetSprites("Units/ShelledParasite.png", "Units/ShelledParasiteBG.png")
                .SetStats(5, 2, 4)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(OnHitEqualHealToSelf.ID, 1),
                        SStack("Shell", 3)
                    };
                });
        }
    }
}
