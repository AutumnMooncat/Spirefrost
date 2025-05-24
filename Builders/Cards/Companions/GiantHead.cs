using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using Spirefrost.Builders.Traits;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.DefectUnits)]
    internal class GiantHead : SpirefrostBuilder
    {
        internal static string ID => "gianthead";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Giant Head")
                .SetSprites("Units/GiantHead.png", "Units/GiantHeadBG2.png")
                .SetStats(10, 3, 0)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.SetWide();
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Card Played Apply Attack To Self", 2),
                        SStack(TriggerWhenStatusAppliedByFriendly.ID, 1)
                    };
                });
        }
    }
}
