using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class IncreaseEffectsWithDesc : SpirefrostBuilder
    {
        internal static string ID => "Increase Effects (With Desc)";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Increase Effects", ID)
                .WithText("Boost the target's effects by <{a}>")
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantIncreaseEffects>(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintCanBeBoosted>()
                    };
                });
        }
    }
}
