using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class BonusDamageEqualToSpiceCustom : SpirefrostBuilder
    {
        internal static string ID => "Bonus Damage Equal To Spice Custom";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectLessJankyBonusDamageEqualToX>(ID)
                .WithText("Deal additional damage equal to <keyword=spice>")
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectLessJankyBonusDamageEqualToX>(data =>
                {
                    data.effectType = "spice";
                    data.on = StatusEffectLessJankyBonusDamageEqualToX.On.Self;
                });
        }
    }
}
