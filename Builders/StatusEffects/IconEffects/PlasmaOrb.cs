using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using Spirefrost.StatusEffects;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.StatusEffects.IconEffects
{
    internal class PlasmaOrb : SpirefrostBuilder
    {
        internal static string ID => "STS Plasma";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static int ApplyAmount => 1;

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectOrb>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(false)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectOrb>(data =>
                {
                    data.passiveEffect = TryGet<StatusEffectData>("Reduce Counter");
                    data.passiveFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.passiveSFXKey = PlasmaIcon.EvokeID;
                    data.evokeEffect = TryGet<StatusEffectData>("Reduce Counter");
                    data.evokeFlags = StatusEffectApplyX.ApplyToFlags.AlliesInRow;
                    data.evokeSFXKey = PlasmaIcon.EvokeID;
                    data.targetConstraints = StatusEffectOrb.OrbConstraints();
                })
                .Subscribe_WithStatusIcon(PlasmaIcon.ID);
        }
    }
}
