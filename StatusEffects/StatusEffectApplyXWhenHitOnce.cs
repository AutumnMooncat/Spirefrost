using System.Collections;
using UnityEngine;

namespace Spirefrost
{
    public class StatusEffectApplyXWhenHitOnce : StatusEffectApplyXWhenHit
    {
        public bool ignoreSilence = true;

        public override void Init()
        {
            base.PostHit += RemoveMe;
        }

        public override bool TargetSilenced()
        {
            if (ignoreSilence)
            {
                return false;
            }
            return base.TargetSilenced();
        }

        public override int GetAmount()
        {
            if (!target || (target.silenced && !ignoreSilence))
            {
                return 0;
            }

            if (!canBeBoosted)
            {
                return count;
            }

            return Mathf.Max(0, Mathf.RoundToInt((float)(count + target.effectBonus) * target.effectFactor));
        }

        public IEnumerator RemoveMe(Hit hit)
        {
            yield return Run(GetTargets(hit, GetTargetContainers(), GetTargetActualContainers()), hit.damage + hit.damageBlocked);
            ActionQueue.Stack(new ActionSequence(CountDown())
            {
                fixedPosition = true,
                note = "Remove Apply When Hit Once"
            });
            yield break;
        }

        public IEnumerator CountDown()
        {
            if ((bool)this && (bool)target && target.alive)
            {
                int amount = GetAmount();
                Events.InvokeStatusEffectCountDown(this, ref amount);
                if (amount != 0)
                {
                    yield return CountDown(target, amount);
                }
            }
        }
    }
}
