using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class InstantHitSelfForXDamage : SpirefrostBuilder
    {
        internal static string ID => "Instant Hit Self For X Damage";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectInstantHitSelfForXDamage>(ID)
                .WithText("Target attacks itself for <{a}> damage")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantHitSelfForXDamage>(data =>
                {

                });
        }
    }
}
