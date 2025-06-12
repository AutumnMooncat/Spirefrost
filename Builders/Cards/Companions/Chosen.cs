using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.Units)]
    internal class Chosen : SpirefrostBuilder
    {
        internal static string ID => "chosen";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Chosen")
                .SetSprites("Units/Chosen.png", "Units/ChosenBG.png")
                .SetStats(8, 2, 4)
                .WithValue(50)
                .WithEyes(FullID, (0.1f, 1.65f, 0.85f, 0.85f, -20f))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("MultiHit", 1),
                        SStack(OnHitDamageDemonizedTarget.ID, 4)
                    };
                });
        }
    }
}
