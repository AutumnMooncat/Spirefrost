using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using Spirefrost.StatusEffects;
using UnityEngine;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.StatusEffects.IconEffects
{
    internal class FrostOrb : SpirefrostBuilder
    {
        internal static string ID => "STS Frost";

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
                    data.passiveType = StatusEffectOrb.PassiveTriggerType.OnHit;
                    data.passiveEffect = TryGet<StatusEffectData>("Frost");
                    data.passiveFlags = StatusEffectApplyX.ApplyToFlags.Attacker;
                    data.passiveSFXKey = FrostIcon.EvokeID;
                    data.evokeFactor = 2f;
                    data.evokeEffect = TryGet<StatusEffectData>("Frost");
                    data.evokeFlags = StatusEffectApplyX.ApplyToFlags.Enemies;
                    data.evokeApplyConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintPseudoFrontEnemy>()
                    };
                    data.evokeSFXKey = FrostIcon.EvokeID;
                    data.targetConstraints = StatusEffectOrb.OrbConstraints();
                })
                .Subscribe_WithStatusIcon(FrostIcon.ID);
        }
    }
}
