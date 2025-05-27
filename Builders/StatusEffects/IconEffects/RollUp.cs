using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using Spirefrost.StatusEffects;
using UnityEngine;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.StatusEffects.IconEffects
{
    internal class RollUp : SpirefrostBuilder
    {
        internal static string ID => "STS Roll Up";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXWhenHitOnce>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenHitOnce>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("Shell");
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintCanBeHit>()
                    };
                })
                .Subscribe_WithStatusIcon(RollUpIcon.ID);
        }
    }
}
