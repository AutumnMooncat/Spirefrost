using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenAttackItemPlayedReduceCounter : SpirefrostBuilder
    {
        internal static string ID => "When Attack Item Played, Reduce Counter";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXWhenAnyCardIsPlayed>(ID)
                .WithText("When an <Item> with <keyword=attack> is played, count down <keyword=counter> by <{a}>")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenAnyCardIsPlayed>(data =>
                {
                    data.targetPlayedCard = false;
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("Reduce Counter");
                    data.triggerConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintIsItem>(),
                        ScriptableObject.CreateInstance<TargetConstraintDoesDamage>()
                    };
                });
        }
    }
}
