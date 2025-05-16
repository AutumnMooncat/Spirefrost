using System.Collections;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectApplyXWhenStatusAppliedToSelf : StatusEffectApplyX
    {
        public override void Init()
        {
            base.PostApplyStatus += Check;
        }

        public override bool RunPostApplyStatusEvent(StatusEffectApply apply)
        {
            if (target.enabled && ShouldApply(apply.effectData))
            {
                return apply.target == target;
            }

            return false;
        }

        private bool ShouldApply(StatusEffectData apply)
        {
            return apply != null && apply.isStatus && apply.visible;
        }

        public IEnumerator Check(StatusEffectApply apply)
        {
            return Run(GetTargets());
        }
    }
}
