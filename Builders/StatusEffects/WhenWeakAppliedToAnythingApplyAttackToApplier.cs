using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenWeakAppliedToAnythingApplyAttackToApplier : SpirefrostBuilder
    {
        internal static string ID => "When Weak Applied To Anything Apply Attack To Applier";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Snow Applied To Anything Gain Attack To Self", ID)
                .WithText($"Whenever anything is {MakeKeywordInsert(WeakKeyword.FullID)}'d, add <+{{a}}><keyword=attack> to the applier")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenYAppliedTo>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Applier;
                    data.effectToApply = TryGet<StatusEffectData>("Increase Attack");
                    data.whenAppliedTypes = new string[]
                    {
                        WeakKeyword.FullID
                    };
                });
        }
    }
}
