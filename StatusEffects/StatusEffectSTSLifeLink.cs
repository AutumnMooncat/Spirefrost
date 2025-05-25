using System.Collections;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectSTSLifeLink : StatusEffectData
    {
        public CardAnimation animation;

        private bool locked;

        public override void Init()
        {
            Events.OnEntityDisplayUpdated += EntityDisplayUpdated;
        }

        public void OnDestroy()
        {
            Events.OnEntityDisplayUpdated -= EntityDisplayUpdated;
        }

        private void EntityDisplayUpdated(Entity entity)
        {
            if (target.hp.current <= 0 && entity == target)
            {
                TryActivate();
            }
        }

        public override bool RunPostHitEvent(Hit hit)
        {
            if (hit.target == target && target.hp.current <= 0)
            {
                TryActivate();
            }

            return false;
        }

        private void TryActivate()
        {
            if (locked)
            {
                return;
            }
            bool flag = true;
            foreach (StatusEffectData statusEffect in target.statusEffects)
            {
                if (statusEffect != this && statusEffect.preventDeath)
                {
                    flag = false;
                    break;
                }
            }

            if (!flag)
            {
                return;
            }

            locked = true;

            ActionQueue.Stack(new ActionSequence(Cleanse())
            {
                priority = 10
            }, fixedPosition: true);

            ActionQueue.Stack(new ActionRefreshPhase(target, animation, false, 0.5f)
            {
                priority = 10
            }, fixedPosition: true);

            ActionQueue.Stack(new ActionSequence(CountDown())
            {
                priority = 10
            }, fixedPosition: true);
        }

        private IEnumerator Cleanse()
        {
            int num = target.statusEffects.Count;
            for (int i = num - 1; i >= 0; i--)
            {
                StatusEffectData statusEffectData = target.statusEffects[i];
                if (statusEffectData.IsNegativeStatusEffect())
                {
                    yield return statusEffectData.Remove();
                }
            }
        }

        private IEnumerator CountDown()
        {
            if ((bool)this && (bool)target && target.alive)
            {
                yield return CountDown(target, 1);
            }

            locked = false;
        }
    }
}
