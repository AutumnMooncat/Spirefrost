using System.Collections;
using UnityEngine;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectApplyXAtHalfHealth : StatusEffectApplyX
    {
        private bool active;

        private int currentHealth;

        public override void Init()
        {
            Events.OnEntityDisplayUpdated += EntityDisplayUpdated;
        }

        public void OnDestroy()
        {
            Events.OnEntityDisplayUpdated -= EntityDisplayUpdated;
        }

        public override bool RunBeginEvent()
        {
            active = true;
            currentHealth = target.hp.current;
            return false;
        }

        public void EntityDisplayUpdated(Entity entity)
        {
            if (active && target.hp.current != currentHealth && entity == target)
            {
                int num = target.hp.current - currentHealth;
                currentHealth = target.hp.current;
                if (num < 0 && target.enabled && !target.silenced && CheckThreshold() && (!targetMustBeAlive || (target.alive && Battle.IsOnBoard(target))))
                {
                    ActionQueue.Stack(new ActionSequence(HealthLost(-num))
                    {
                        note = base.name,
                        priority = eventPriority
                    }, fixedPosition: true);
                }
            }
        }

        public bool CheckThreshold()
        {
            return target.hp.current <= Mathf.FloorToInt(target.hp.max * 0.5f);
        }

        public IEnumerator HealthLost(int amount)
        {
            if ((bool)this && target.IsAliveAndExists())
            {
                yield return Run(GetTargets(), amount);
            }
        }
    }
}
