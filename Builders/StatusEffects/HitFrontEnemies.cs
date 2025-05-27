using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class HitFrontEnemies : SpirefrostBuilder
    {
        internal static string ID => "Hit Front Enemies";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Hit All Enemies", ID)
                .WithText("Hits enemies in front")
                .SubscribeToAfterAllBuildEvent<StatusEffectChangeTargetMode>(data =>
                {
                    TargetModeAll mode = ScriptableObject.CreateInstance<TargetModeAll>();
                    mode.constraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintFrontUnit>()
                    };
                    data.targetMode = mode;
                });
        }
    }
}
