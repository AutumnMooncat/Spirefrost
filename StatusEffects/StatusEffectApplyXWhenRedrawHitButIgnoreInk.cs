using UnityEngine;

namespace Spirefrost.StatusEffects
{
    public class StatusEffectApplyXWhenRedrawHitButIgnoreInk : StatusEffectApplyXWhenRedrawHit
    {
        public override bool TargetSilenced()
        {
            return false;
        }

        public override int GetAmount()
        {
            if (!target)
            {
                return 0;
            }

            if (!canBeBoosted)
            {
                return count;
            }

            return Mathf.Max(0, Mathf.RoundToInt((count + target.effectBonus) * target.effectFactor));
        }

        public override bool CanTrigger()
        {
            if (target.enabled)
            {
                return true;
            }

            return false;
        }
    }
}
