using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnHitDamageIfDrawPileEmpty : SpirefrostBuilder
    {
        internal static string ID => "On Hit Damage If Draw Pile Empty";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Hit Damage Damaged Target", ID)
                .WithText("Deal <{a}> additional damage if your draw pile is empty")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnHit>(data =>
                {
                    TargetConstraintEmptyPile emptyDraw = ScriptableObject.CreateInstance<TargetConstraintEmptyPile>();
                    emptyDraw.pile = TargetConstraintEmptyPile.PileType.Draw;
                    data.applyConstraints = new TargetConstraint[]
                    {
                        emptyDraw
                    };
                });
        }
    }
}
