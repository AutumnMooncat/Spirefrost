using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class SetAttackOfCardInHand : SpirefrostBuilder
    {
        internal static string ID => "Set Attack Of Card In Hand";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Set Attack", ID)
                .WithText("Set <keyword=attack> of a card in your hand to <{a}>")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantSetAttack>(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintDoesDamage>()
                    };
                });
        }
    }
}
