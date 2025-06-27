using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class InstantApplyCurrentShroomToRandomEnemy : SpirefrostBuilder
    {
        internal static string ID => "Instant Apply Current Shroom To Random Enemy";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Instant Apply Current Attack To Random Ally", ID)
                .WithText("Copy the target's <keyword=shroom> to a random enemy")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXInstant>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(InstantApplyCurrentShroomToRandomEnemyCallback.ID);
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Applier;
                    ScriptableCurrentStatus script = ScriptableObject.CreateInstance<ScriptableCurrentStatus>();
                    data.scriptableAmount = script;
                    data.targetConstraints = new TargetConstraint[0];
                    data.applyConstraints = new TargetConstraint[0];
                });
        }
    }

    internal class InstantApplyCurrentShroomToRandomEnemyCallback : SpirefrostBuilder
    {
        internal static string ID => "Instant Apply Current Shroom To Random Enemy (Callback)";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXInstant>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXInstant>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Shroom");
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomEnemy;
                });
        }
    }
}
