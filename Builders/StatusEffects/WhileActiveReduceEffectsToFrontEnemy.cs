using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhileActiveReduceEffectsToFrontEnemy : SpirefrostBuilder
    {
        internal static string ID => "While Active Reduce Effects To FrontEnemy";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("While Active Increase Effects To FrontAlly", ID)
                .WithText("While active, reduce the effects of the front enemy by <{a}>")
                .WithStackable(true)
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectWhileActiveX>(data => {
                    data.effectToApply = TryGet<StatusEffectData>("Ongoing Reduce Effects");
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.EnemiesInRow;
                    data.applyConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintFrontUnit>(),
                    };
                });
        }
    }
}
