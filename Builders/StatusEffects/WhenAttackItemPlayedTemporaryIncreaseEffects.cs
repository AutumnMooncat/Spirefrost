using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenAttackItemPlayedTemporaryIncreaseEffects : SpirefrostBuilder
    {
        internal static string ID => "When Attack Item Played Temporary Increase Effects";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXWhenAnyCardIsPlayed>(ID)
                .WithText("When an <Item> with <keyword=attack> is played, increase by {a} until played")
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenAnyCardIsPlayed>(data =>
                {
                    data.undoAppliedAfterTrigger = true;
                    data.worksAnywhere = true;
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("Increase Attack Effects");
                    data.triggerConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintIsItem>(),
                        ScriptableObject.CreateInstance<TargetConstraintDoesDamage>()
                    };
                });
        }
    }
}
