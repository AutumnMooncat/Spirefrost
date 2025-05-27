using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using Spirefrost.StatusEffects;
using UnityEngine;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.StatusEffects.IconEffects
{
    internal class Amplify : SpirefrostBuilder
    {
        internal static string ID => "STS Amplify";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectSTSAmplify>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectSTSAmplify>(data =>
                {
                    TargetConstraintMaxCounterMoreThan hasCounter = ScriptableObject.CreateInstance<TargetConstraintMaxCounterMoreThan>();
                    hasCounter.moreThan = 0;
                    TargetConstraintOr doesTrigger = ScriptableObject.CreateInstance<TargetConstraintOr>();
                    doesTrigger.constraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintIsItem>(),
                        ScriptableObject.CreateInstance<TargetConstraintHasReaction>(),
                        hasCounter
                    };
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintCanBeBoosted>(),
                        doesTrigger
                    };
                })
                .Subscribe_WithStatusIcon(AmplifyIcon.ID);
        }
    }
}
