using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class AttackAboveXCountsAsFrenzy : SpirefrostBuilder
    {
        internal static string ID => "Attack Above X Counts As Frenzy";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectTempConvertAttackToYPreTrigger>(ID)
                .WithText("<keyword=attack> above <{a}> counts as <keyword=frenzy>")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectTempConvertAttackToYPreTrigger>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("MultiHit");
                    data.oncePerTurn = true;
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintDoesDamage>()
                    };
                });
        }
    }
}
