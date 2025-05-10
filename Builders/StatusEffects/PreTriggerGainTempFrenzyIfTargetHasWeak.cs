using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects.IconEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class PreTriggerGainTempFrenzyIfTargetHasWeak : SpirefrostBuilder
    {
        internal static string ID => "Pre Trigger Gain Temp Frenzy If Target Has Weak";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyTempXForTrigger>(ID)
                .WithText($"Before triggering, gain <x{{a}}> temporary <keyword=frenzy> if the target has {MakeKeywordInsert(WeakKeyword.FullID)}")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyTempXForTrigger>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("MultiHit");
                    data.doPing = true;
                    data.targetApplyConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintHasStatus>(t => t.status = TryGet<StatusEffectData>(Weak.ID))
                    };
                });
        }
    }
}
