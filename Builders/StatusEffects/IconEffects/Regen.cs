using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using Spirefrost.StatusEffects;
using UnityEngine;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.StatusEffects.IconEffects
{
    internal class Regen : SpirefrostBuilder
    {
        internal static string ID => "STS Regen";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXAfterTurnAndDecay>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXAfterTurnAndDecay>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Heal");
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintHasHealth>()
                    };
                })
                .Subscribe_WithStatusIcon(RegenIcon.ID);
        }
    }
}
