using Spirefrost.Patches;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Spirefrost.StatusEffects
{
    public class StatusEffectOrb : StatusEffectApplyX, INonStackingStatusEffect
    {
        public enum PassiveTriggerType
        {
            PerTurn,
            OnHit
        }

        public int passiveIncrease;

        public PassiveTriggerType passiveType = PassiveTriggerType.PerTurn;

        public StatusEffectData passiveEffect;

        public ApplyToFlags passiveFlags;

        public TargetConstraint[] passiveApplyConstraints;

        public string passiveSFXKey;

        public float evokeFactor = 1f;

        public StatusEffectData evokeEffect;

        public ApplyToFlags evokeFlags;

        public TargetConstraint[] evokeApplyConstraints;

        public string evokeSFXKey;

        public StatusIcon Icon { get => _icon; set => _icon = value; }

        private StatusIcon _icon;

        public override void Init()
        {
            OnTurnEnd += PassiveTurnEndTrigger;
            PostHit += PassiveHitTrigger;
            PreTrigger += EvokePreTrigger;
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

            return Mathf.Max(0, Mathf.RoundToInt((count + target.effectBonus) * target.effectFactor));
        }

        public override bool RunTurnEndEvent(Entity entity)
        {
            if (target.enabled && entity == target && Battle.IsOnBoard(target))
            {
                return passiveType == PassiveTriggerType.PerTurn;
            }

            return false;
        }

        public IEnumerator PassiveTurnEndTrigger(Entity entity)
        {
            if (passiveEffect)
            {
                SetToPassive();
                if (!passiveSFXKey.IsNullOrEmpty())
                {
                    SpirefrostUtils.PlayGlobalSound(passiveSFXKey);
                }
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
            if (target.enabled && hit.target == target && hit.canRetaliate && (!targetMustBeAlive || target.alive && Battle.IsOnBoard(target)) && hit.Offensive && hit.BasicHit)
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
                if (!passiveSFXKey.IsNullOrEmpty())
                {
                    SpirefrostUtils.PlayGlobalSound(passiveSFXKey);
                }
                yield return Run(GetTargets(hit, GetTargetContainers(), GetTargetActualContainers()));
            }
            if (passiveIncrease != 0)
            {
                count += passiveIncrease;
                target.PromptUpdate();
            }
        }

        private IEnumerator EvokePreTrigger(Trigger trigger)
        {
            if (trigger.entity == target && trigger.countsAsTrigger && !AlreadyAttackingPatches.HasAlreadyAttacked(target))
            {
                bool hadTargets = !(trigger.targets is null) && trigger.targets.Any(e => e.IsAliveAndExists());
                if (evokeEffect)
                {
                    SetToEvoke();
                    if (!evokeSFXKey.IsNullOrEmpty())
                    {
                        SpirefrostUtils.PlayGlobalSound(evokeSFXKey);
                    }
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
                bool hasTargets = !(trigger.targets is null) && trigger.targets.Any(e => e.IsAliveAndExists());

                if (hadTargets != hasTargets)
                {
                    trigger.targets = target.targetMode ? target.targetMode.GetTargets(target, null, null) : null;
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
