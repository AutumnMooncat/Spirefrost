using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenHitApplyShroomToEnemies : SpirefrostBuilder
    {
        internal static string ID => "When Hit Apply Shroom To Enemies";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Hit Apply Frost To Enemies", ID)
                .WithText("When hit, apply <{a}><keyword=shroom> to all enemies")
                .WithCanBeBoosted(true)
                .WithOffensive(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenHit>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Shroom");
                    data.applyConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintCanBeHit>()
                    };
                    data.noTargetType = NoTargetType.NoTargetForStatus;
                    data.noTargetTypeArgs = new string[] { "<sprite name=shroom>" };
                });
        }
    }
}
