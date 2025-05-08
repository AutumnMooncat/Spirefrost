using Spirefrost.Patches;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectApplyXWhenNonTempYAppliedToSelf : StatusEffectApplyXWhenYAppliedToSelf
    {
        public override bool RunPostApplyStatusEvent(StatusEffectApply apply)
        {
            if (StatusSystemPatch.isTemp)
            {
                return false;
            }
            return base.RunPostApplyStatusEvent(apply);
        }
    }
}
