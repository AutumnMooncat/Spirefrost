using System;
using System.Collections;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectSTSIntangible : StatusEffectData
    {
        public override void Init()
        {
            SpirefrostEvents.OnIgnoreTriggerCheck += IgnoreTrigger;
        }

        public void OnDestroy()
        {
            SpirefrostEvents.OnIgnoreTriggerCheck -= IgnoreTrigger;
        }

        public override bool RunBeginEvent()
        {
            target.cannotBeHitCount++;
            return false;
        }

        public override bool RunEndEvent()
        {
            target.cannotBeHitCount--;
            return false;
        }

        public override bool RunHitEvent(Hit hit)
        {
            if (hit.target == target && hit.damage > 0)
            {
                hit.damageBlocked += hit.damage;
                hit.damage = 0;
            }

            return false;
        }

        private void IgnoreTrigger(ref Trigger trigger, ref bool ignore)
        {
            if (trigger.targets != null && trigger.targets.Contains(target))
            {
                if (trigger.entity.HasAttackIcon())
                {
                    trigger.targets = trigger.targets.Without(target);

                    if (trigger.targets.Length == 0)
                    {
                        ignore = true;
                    }
                }
            }
        }
    }
}
