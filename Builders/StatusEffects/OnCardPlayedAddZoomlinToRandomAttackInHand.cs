using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnCardPlayedAddZoomlinToRandomAttackInHand : SpirefrostBuilder
    {
        internal static string ID => "On Card Played Add Zoomlin To Random Attack In Hand";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Card Played Add Zoomlin To Random Card In Hand", ID)
                .WithText("Add <keyword=zoomlin> to a random <Item> with <keyword=attack> in your hand")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    TargetConstraintAnd damageItem = ScriptableObject.CreateInstance<TargetConstraintAnd>();
                    damageItem.constraints = new TargetConstraint[] {
                        ScriptableObject.CreateInstance<TargetConstraintDoesDamage>(),
                        ScriptableObject.CreateInstance<TargetConstraintIsItem>()
                    };
                    data.applyConstraints = new TargetConstraint[]
                    {
                        damageItem
                    };
                });
        }
    }
}
