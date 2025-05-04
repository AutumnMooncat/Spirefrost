using System.Collections;

namespace Spirefrost
{
    public class StatusEffectSTSDoubleTap : StatusEffectData
    {
        public bool cardPlayed;

        public int current;

        public int amountToClear;

        public StatusEffectData effectToApply;

        public override void Init()
        {
            base.OnActionPerformed += ActionPerformed;
            base.OnStack += StackRoutine;
            base.OnEnd += EndRoutine;
            SpirefrostEvents.OnMovedByDiscarder += DiscardCheck;
        }

        public void OnDestroy()
        {
            SpirefrostEvents.OnMovedByDiscarder -= DiscardCheck;
        }

        private void DiscardCheck(Entity entity)
        {
            if (entity == target && cardPlayed && amountToClear != 0)
            {
                cardPlayed = false;
                ActionQueue.Stack(new ActionSequence(Clear(amountToClear))
                {
                    note = "Clear Double Tap"
                }, fixedPosition: true);
            }
        }

        public override bool RunStackEvent(int stacks)
        {
            current += stacks;
            return true;
        }

        public override IEnumerator StackRoutine(int stacks)
        {
            yield return ModifyFrenzyStacks(stacks);
        }

        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            if (!cardPlayed && entity == target && count > 0)
            {
                cardPlayed = true;
                amountToClear = current;
            }

            return false;
        }

        public override bool RunActionPerformedEvent(PlayAction action)
        {
            if (cardPlayed)
            {
                return ActionQueue.Empty;
            }

            return false;
        }

        public IEnumerator ActionPerformed(PlayAction action)
        {
            cardPlayed = false;
            yield return Clear(amountToClear);
        }

        public IEnumerator Clear(int amount)
        {
            amountToClear = 0;
            Events.InvokeStatusEffectCountDown(this, ref amount);
            if (amount != 0)
            {
                current -= amount;
                yield return ModifyFrenzyStacks(-amount);
                yield return CountDownForced(amount);
            }
        }

        public IEnumerator CountDownForced(int amount)
        {
            if ((bool)target)
            {
                yield return RemoveStacks(amount, removeTemporary: false);
            }
        }

        public override IEnumerator EndRoutine()
        {
            yield return ModifyFrenzyStacks(-current);
        }

        private IEnumerator ModifyFrenzyStacks(int stacks)
        {
            if (stacks > 0)
            {
                target.curveAnimator.Ping();
                yield return StatusEffectSystem.Apply(target, target, effectToApply, stacks, temporary: true);
                target.PromptUpdate();
            }
            else if (stacks < 0)
            {
                StatusEffectData frenzyEffect = target.statusEffects.Find((StatusEffectData a) => a.name.Equals(effectToApply.name));
                target.curveAnimator.Ping();
                yield return frenzyEffect?.RemoveStacks(-stacks, removeTemporary: true);
            }
        }
    }
}
