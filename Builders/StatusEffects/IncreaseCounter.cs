using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class IncreaseCounter : SpirefrostBuilder
    {
        internal static string ID => "Increase Counter";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectInstantIncreaseCounter>(ID)
                .WithText("Count up <keyword=counter> by <{a}>")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantIncreaseCounter>(data =>
                {
                    TargetConstraintMaxCounterMoreThan hasCounter = ScriptableObject.CreateInstance<TargetConstraintMaxCounterMoreThan>();
                    hasCounter.moreThan = 0;
                    TargetConstraintHasStatus doesntHaveSnow = ScriptableObject.CreateInstance<TargetConstraintHasStatus>();
                    doesntHaveSnow.not = true;
                    doesntHaveSnow.status = TryGet<StatusEffectData>("Snow");
                    data.targetConstraints = new TargetConstraint[]
                    {
                        hasCounter,
                        doesntHaveSnow,
                        ScriptableObject.CreateInstance<TargetConstraintOnBoard>()
                    };
                });
        }
    }
}
