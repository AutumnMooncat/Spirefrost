using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class ReduceAttackWithText : SpirefrostBuilder
    {
        internal static string ID => "Reduce Attack With Text";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Reduce Attack", ID)
                .WithText("Reduce <keyword=attack> by <{a}>")
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantReduceAttack>(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintDoesDamage>()
                    };
                });
        }
    }
}
