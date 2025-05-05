using System.Collections;
using UnityEngine;

namespace Spirefrost
{
    public class StatusEffectOrb : StatusEffectApplyX
    {
        public enum PassiveTriggerType
        {
            PerTurn,
            OnHit
        }

        private bool subbed;

        private bool primed;

        public int passiveIncrease;

        public PassiveTriggerType passiveType = PassiveTriggerType.PerTurn;

        public StatusEffectData passiveEffect;

        public ApplyToFlags passiveFlags;

        public TargetConstraint[] passiveApplyConstraints;

        public float evokeFactor = 1f;

        public StatusEffectData evokeEffect;

        public ApplyToFlags evokeFlags;

        public TargetConstraint[] evokeApplyConstraints;

        public override void Init()
        {
            base.OnTurnEnd += PassiveTurnTrigger;
            base.PostHit += PassiveHitTrigger;
            base.PreTrigger += EvokeTrigger;
            Events.OnPostProcessUnits += Prime;
            subbed = true;
        }

        public void OnDestroy()
        {
            Unsub();
        }

        public void Unsub()
        {
            if (subbed)
            {
                Events.OnPostProcessUnits -= Prime;
                subbed = false;
            }
        }

        public void Prime(Character character)
        {
            primed = true;
            Unsub();
        }

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

            return Mathf.Max(0, Mathf.RoundToInt((float)(count + target.effectBonus) * target.effectFactor));
        }

        public override bool RunTurnEndEvent(Entity entity)
        {
            if (primed && target.enabled && entity == target && Battle.IsOnBoard(target))
            {
                return passiveType == PassiveTriggerType.PerTurn;
            }

            return false;
        }

        public IEnumerator PassiveTurnTrigger(Entity entity)
        {
            if (passiveEffect)
            {
                SetToPassive();
                yield return Run(GetTargets());
            }
            if (passiveIncrease != 0)
            {
                count += passiveIncrease;
                target.PromptUpdate();
            }
        }

        public override bool RunPostHitEvent(Hit hit)
        {
            if (target.enabled && hit.target == target && hit.canRetaliate && (!targetMustBeAlive || (target.alive && Battle.IsOnBoard(target))) && hit.Offensive && hit.BasicHit)
            {
                return passiveType == PassiveTriggerType.OnHit;
            }

            return false;
        }

        public IEnumerator PassiveHitTrigger(Hit hit)
        {
            if (passiveEffect)
            {
                SetToPassive();
                yield return Run(GetTargets(hit, GetTargetContainers(), GetTargetActualContainers()));
            }
            if (passiveIncrease != 0)
            {
                count += passiveIncrease;
                target.PromptUpdate();
            }
        }

        private IEnumerator EvokeTrigger(Trigger trigger)
        {
            if (primed && trigger.entity == target && trigger.countsAsTrigger)
            {
                if (evokeEffect)
                {
                    SetToEvoke();
                    int originalAmount = count;
                    count = Mathf.CeilToInt(count * evokeFactor);
                    yield return Run(GetTargets());
                    count = originalAmount;
                }
                // Still remove Dark at 0
                int amount = Mathf.Max(count, 1);
                Events.InvokeStatusEffectCountDown(this, ref amount);
                if (amount != 0)
                {
                    yield return CountDown(target, amount);
                }
            }
        }

        private void SetToPassive()
        {
            effectToApply = passiveEffect;
            applyToFlags = passiveFlags;
            applyConstraints = passiveApplyConstraints;
        }

        private void SetToEvoke()
        {
            effectToApply = evokeEffect;
            applyToFlags = evokeFlags;
            applyConstraints = evokeApplyConstraints;
        }
    }
}
