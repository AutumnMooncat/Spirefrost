using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects.IconEffects;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnCardPlayedApplyDoubleTapToRandomAttackInHand : SpirefrostBuilder
    {
        internal static string ID => "On Card Played Apply Double Tap To Random Attack In Hand";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Card Played Add Zoomlin To Random Card In Hand", ID)
                .WithText($"Apply <{{a}}>{MakeKeywordInsert(DoubleTapKeyword.FullID)} to a random {MakeKeywordInsert(AttackKeyword.FullID)} in your hand")
                .WithCanBeBoosted(true)
                .WithStackable(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(DoubleTap.ID);
                    data.applyConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintDoesDamage>(),
                        ScriptableObject.CreateInstance<TargetConstraintIsItem>()
                    };
                    data.noTargetType = NoTargetType.NoTargetForStatus;
                    data.noTargetTypeArgs = new string[] { MakeSpriteInsert(DoubleTapIcon.SpriteID) };
                });
        }
    }
}
