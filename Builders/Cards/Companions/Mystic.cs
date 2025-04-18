using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;

namespace Spirefrost.Builders.Cards.Companions
{
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
                        SStack(OnTurnApplyRegenToAllies.ID, 2),
                    };
                });
        }
    }
}
