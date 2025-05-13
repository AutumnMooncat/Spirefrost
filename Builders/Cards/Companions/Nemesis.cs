using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using Spirefrost.Builders.StatusEffects.IconEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.SilentUnits)]
    internal class Nemesis : SpirefrostBuilder
    {
        internal static string ID => "nemesis";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Nemesis")
                .SetSprites("Units/Nemesis.png", "Units/NemesisBG.png")
                .SetStats(1, 3, 5)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(TriggerWhenShroomApplied.ID, 1),
                        SStack(Intangible.ID, 1)
                    };
                });
        }
    }
}
