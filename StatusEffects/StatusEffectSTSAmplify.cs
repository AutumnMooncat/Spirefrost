using System.Collections;

namespace Spirefrost.StatusEffects
{
    public class StatusEffectSTSAmplify : StatusEffectData
    {
        public bool cardPlayed;

        public int current;

        public int amountToClear;

        public override void Init()
        {
            OnActionPerformed += ActionPerformed;
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
                    note = "Clear Amplify"
                }, fixedPosition: true);
            }
        }

        public override bool RunStackEvent(int stacks)
        {
            current += stacks;
            target.effectBonus += stacks;
            target.curveAnimator.Ping();
            target.PromptUpdate();
            return false;
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
                target.effectBonus -= amount;
                target.PromptUpdate();
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

        public override bool RunEndEvent()
        {
            target.effectBonus -= current;
            target.PromptUpdate();
            return false;
        }
    }
}
