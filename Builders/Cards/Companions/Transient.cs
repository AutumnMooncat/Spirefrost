using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using Spirefrost.Builders.StatusEffects.IconEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.DefectUnits)]
    internal class Transient : SpirefrostBuilder
    {
        internal static string ID => "transient";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Transient")
                .SetSprites("Units/Transient.png", "Units/TransientBG.png")
                .SetStats(10, 8, 6)
                .WithValue(50)
                .WithEyes(FullID, (0.035f, 0.75f, 1.5f, 1.5f, 0f))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(WhenHitApplyFrostToSelf.ID, 2),
                        SStack(Fading.ID, 3)
                    };
                });
        }
    }
}
