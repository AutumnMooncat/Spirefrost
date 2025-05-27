using System.Collections;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectApplyTempXForTrigger : StatusEffectData
    {
        public StatusEffectData effectToApply;

        public ScriptableAmount scriptableAmount;

        public TargetConstraint[] selfApplyConstraints;

        public TargetConstraint[] targetApplyConstraints;

        public bool doPing;

        private bool cardPlayed;

        private int appliedAmount;

        private bool runThisTurn;

        public override void Init()
        {
            PreTrigger += EntityPreTrigger;
            OnActionPerformed += ActionPerformed;
        }

        public override bool RunPreTriggerEvent(Trigger trigger)
        {
            return CheckTrigger(trigger);
        }

        private bool CheckTrigger(Trigger trigger)
        {
            if (runThisTurn || !target.enabled || trigger.entity != target || target.silenced)
            {
                return false;
            }

            foreach (var constr in selfApplyConstraints)
            {
                if (!constr.Check(target))
                {
                    return false;
                }
            }

            foreach (var entity in trigger.targets)
            {
                foreach (var constr in targetApplyConstraints)
                {
                    if (!constr.Check(entity))
                    {
                        break;
                    }
                }
                return true;
            }

            return targetApplyConstraints.Length == 0;
        }

        private IEnumerator EntityPreTrigger(Trigger trigger)
        {
            runThisTurn = true;
            appliedAmount = scriptableAmount ? scriptableAmount.Get(target) : GetAmount();
            if (appliedAmount > 0)
            {
                if (doPing)
                {
                    target.curveAnimator.Ping();
                }
                yield return StatusEffectSystem.Apply(target, applier, effectToApply, appliedAmount, true);
            }
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

        private IEnumerator ActionPerformed(PlayAction action)
        {
            cardPlayed = false;
            runThisTurn = false;
            if (appliedAmount > 0)
            {
                StatusEffectData addedEffect = target.statusEffects.Find((a) => a.name.Equals(effectToApply.name));
                if (doPing)
                {
                    target.curveAnimator.Ping();
                }
                yield return addedEffect?.RemoveStacks(appliedAmount, removeTemporary: true);
            }
        }
    }
}
