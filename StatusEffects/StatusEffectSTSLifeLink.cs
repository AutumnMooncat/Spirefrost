using System.Collections;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectSTSLifeLink : StatusEffectData
    {
        public CardAnimation animation;

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

            FindObjectOfType<ChangePhaseAnimationSystem>()?.Flash();
            IEnumerator logic = ClumpLogic();
            while (logic.MoveNext())
            {
                object _ = logic.Current;
            }
            ActionQueue.Stack(new ActionRefreshPhase(target, animation, false, 0.5f)
            {
                priority = 10
            }, fixedPosition: true);
            return;
        }

        private IEnumerator ClumpLogic()
        {
            Routine.Clump clump = new Routine.Clump();
            clump.Add(Logic());
            yield return clump.WaitForEnd();
        }

        private IEnumerator Logic()
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

            if ((bool)this && (bool)target && target.alive)
            {
                yield return CountDown(target, 1);
            }
        }
    }
}
