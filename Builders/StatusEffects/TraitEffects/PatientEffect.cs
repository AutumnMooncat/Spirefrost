using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects.TraitEffects
{
    internal class PatientEffect : SpirefrostBuilder
    {
        internal static string ID => "Patient Effect";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXWhenRetained>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenRetained>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("Temporary Zoomlin");
                });
        }
    }
}
