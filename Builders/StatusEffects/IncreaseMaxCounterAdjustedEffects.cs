using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenHitIncreaseAttackerCounterAdjusted : SpirefrostBuilder
    {
        internal static string ID => "When Hit Increase Attacker Counter Adjusted";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Hit Apply Snow To Attacker", ID)
                .WithText("When hit, increase attacker's <keyword=counter> by {a}")
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenHit>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(IncreaseMaxCounterAdjusted.ID);
                });
        }
    }

    internal class IncreaseMaxCounterAdjusted : SpirefrostBuilder
    {
        internal static string ID => "Increase Max Counter Adjusted";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectInstantIncreaseMaxCounterAdjusted>(ID)
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantIncreaseMaxCounterAdjusted>(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintMaxCounterMoreThan>(),
                    };
                });
        }
    }
}
