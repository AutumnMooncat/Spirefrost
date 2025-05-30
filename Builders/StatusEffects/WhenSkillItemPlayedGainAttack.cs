﻿using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenSkillItemPlayedGainAttack : SpirefrostBuilder
    {
        internal static string ID => "When Skill Item Played, Gain Attack";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXWhenAnyCardIsPlayed>(ID)
                .WithText($"When a {MakeKeywordInsert(SkillKeyword.FullID)} is played, gain <+{{a}}><keyword=attack>")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenAnyCardIsPlayed>(data =>
                {
                    data.targetPlayedCard = false;
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("Increase Attack");
                    data.triggerConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintIsItem>(),
                        MakeConstraint<TargetConstraintDoesDamage>(c => c.not = true)
                    };
                });
        }
    }
}
