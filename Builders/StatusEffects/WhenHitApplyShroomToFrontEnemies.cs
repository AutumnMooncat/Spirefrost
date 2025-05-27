using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenHitApplyShroomToFrontEnemies : SpirefrostBuilder
    {
        internal static string ID => "When Hit Apply Shroom To Front Enemies";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXToFrontEnemiesWhenHit>(ID)
                .WithText("When hit, apply <{a}><keyword=shroom> to enemies in front")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXToFrontEnemiesWhenHit>(data =>
                {
                    data.doesDamage = true;
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
