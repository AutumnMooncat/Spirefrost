using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class HalveHealthApplyAttackEffect : SpirefrostBuilder
    {
        internal static string ID => "Halve Health Apply Attack Effect";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectInstantHalveHealthApplyEqualX>(ID)
                .WithText("Convert half of current <keyword=health> into <keyword=attack>")
                .WithCanBeBoosted(false)
                .WithStackable(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantHalveHealthApplyEqualX>(data =>
                {
                    data.equalToLostHealth = true;
                    data.effectToApply = TryGet<StatusEffectData>("Increase Attack");
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintHasHealth>(),
                        MakeConstraint<TargetConstraintDoesDamage>()
                    };
                });
        }
    }
}
