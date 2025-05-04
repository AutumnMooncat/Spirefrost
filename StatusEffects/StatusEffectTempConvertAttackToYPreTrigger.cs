using System.Collections;
using UnityEngine;

namespace Spirefrost
{
    public class StatusEffectTempConvertAttackToYPreTrigger : StatusEffectApplyXPreTrigger
    {
        private int removedTemp;

        private int removedCurrent;

        private int removedMax;

        private int effectAmountAdded;

        private bool cardPlayed;

        public StatusEffectTempConvertAttackToYPreTrigger()
        {
            scriptableAmount = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
            ((ScriptableFixedAmount)scriptableAmount).amount = 0;
            eventPriority = -10000;
        }

        public override void Init()
        {
            base.PreTrigger += EntityPreTrigger;
            base.OnActionPerformed += ActionPerformed;
            SpirefrostEvents.OnMovedByDiscarder += DiscardCheck;
        }

        public void OnDestroy()
        {
            SpirefrostEvents.OnMovedByDiscarder -= DiscardCheck;
        }

        private void DiscardCheck(Entity entity)
        {
            if (entity == target && cardPlayed)
            {
                cardPlayed = false;
                hasRunThisTurn = false;
                ActionQueue.Stack(new ActionSequence(ActionPerformed(null))
                {
                    note = "Clear Temp Convert"
                }, fixedPosition: true);
            }
        }

        public new IEnumerator EntityPreTrigger(Trigger trigger)
        {
            if (oncePerTurn)
            {
                hasRunThisTurn = true;
            }

            running = true;
            ((ScriptableFixedAmount)scriptableAmount).amount = 0;
            int reduceTo = GetAmount();
            bool prompt = false;
            if (target.damage.current + target.tempDamage.Value > reduceTo)
            {
                // Current damage goes to reduceTo if higher than it
                // Temp damage needs to be set so damage+temp = reduceTo
                ((ScriptableFixedAmount)scriptableAmount).amount = target.damage.current + target.tempDamage.Value - reduceTo;
                if (target.damage.current > reduceTo)
                {
                    removedCurrent = target.damage.current - reduceTo;
                    target.damage.current = reduceTo;
                    prompt = true;
                }

                removedTemp = target.tempDamage.Value - (reduceTo - target.damage.current);
                target.tempDamage.Value = reduceTo - target.damage.current;
            }

            // Also reduce max if applicable
            if (target.damage.max > reduceTo)
            {
                removedMax = target.damage.max - reduceTo;
                target.damage.max = reduceTo;
                prompt = true;
            }
            if (prompt)
            {
                target.PromptUpdate();
            }
            if (((ScriptableFixedAmount)scriptableAmount).amount > 0)
            {
                effectAmountAdded += ((ScriptableFixedAmount)scriptableAmount).amount;
                yield return Run(runAgainst);
            }
            runAgainst = null;
            running = false;
        }

        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            if (!cardPlayed && entity == target)
            {
                cardPlayed = true;
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
            /*UnityEngine.Debug.Log($"{MainModFile.instance.Title} - Clearing temp stacks?");
            UnityEngine.Debug.Log($"{MainModFile.instance.Title} - Removed current: {removedCurrent}");
            UnityEngine.Debug.Log($"{MainModFile.instance.Title} - Removed max: {removedMax}");
            UnityEngine.Debug.Log($"{MainModFile.instance.Title} - Removed temp: {removedTemp}");
            UnityEngine.Debug.Log($"{MainModFile.instance.Title} - Effect added: {effectAmountAdded}");*/
            cardPlayed = false;
            target.damage.current += removedCurrent;
            target.damage.max += removedMax;
            target.tempDamage.Value += removedTemp;
            if (removedCurrent != 0 || removedMax != 0)
            {
                target.PromptUpdate();
            }
            removedCurrent = 0;
            removedMax = 0;
            removedTemp = 0;
            if (effectAmountAdded > 0)
            {
                StatusEffectData addedEffect = target.statusEffects.Find((StatusEffectData a) => a.name.Equals(effectToApply.name));
                if (doPing)
                {
                    target.curveAnimator.Ping();
                }
                yield return addedEffect?.RemoveStacks(effectAmountAdded, removeTemporary: false);
            }
            effectAmountAdded = 0;
            yield break;
        }
    }
}
