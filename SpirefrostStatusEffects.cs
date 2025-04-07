using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Spirefrost
{
    public class StatusEffectApplyXAfterTurnAndDecay : StatusEffectApplyX
    {
        public bool subbed;

        public bool primed;

        public override void Init()
        {
            base.OnTurnEnd += PostTurn;
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

        public override bool RunTurnEndEvent(Entity entity)
        {
            if (primed && target.enabled)
            {
                return entity == target;
            }

            return false;
        }

        public IEnumerator PostTurn(Entity entity)
        {
            ActionQueue.Stack(new ActionSequence(CountDown())
            {
                fixedPosition = true,
                note = "Decay After Turn"
            });
            yield return Run(GetTargets());
        }

        public IEnumerator CountDown()
        {
            if ((bool)this && (bool)target && target.alive)
            {
                int amount = 1;
                Events.InvokeStatusEffectCountDown(this, ref amount);
                if (amount != 0)
                {
                    yield return CountDown(target, amount);
                }
            }
        }
    }

    public class StatusEffectSTSVulnerable : StatusEffectData
    {
        public int amountToClear;

        public StatusEffectSTSVulnerable()
        {
            removeOnDiscard = true;
        }

        public override void Init()
        {
            base.OnHit += MultiplyHit;
        }

        public override bool RunHitEvent(Hit hit)
        {
            if (hit.Offensive && count > 0 && hit.damage > 0)
            {
                return hit.target == target;
            }

            return false;
        }

        public IEnumerator MultiplyHit(Hit hit)
        {
            amountToClear = GetAmount();
            hit.damage = Mathf.CeilToInt(hit.damage * (1 + (amountToClear * 0.5f)));
            ActionQueue.Stack(new ActionSequence(Clear(amountToClear))
            {
                fixedPosition = true,
                note = "Clear Vulnerable"
            });
            yield break;
        }

        public IEnumerator Clear(int clearMe)
        {
            if ((bool)this && (bool)target && target.alive)
            {
                int amount = clearMe;
                Events.InvokeStatusEffectCountDown(this, ref amount);
                if (amount != 0)
                {
                    yield return CountDown(target, amount);
                }
            }
        }
    }

    public class StatusEffectSTSWeakness : StatusEffectData
    {
        private int toClear;

        public StatusEffectSTSWeakness()
        {
            eventPriority = -999999;
            removeOnDiscard = true;
        }

        public override void Init()
        {
            base.OnActionPerformed += ActionPerformed;
            base.OnHit += HalveDamage;
        }

        public override bool RunHitEvent(Hit hit)
        {
            if (hit.attacker == target && hit.countsAsHit)
            {
                return true;
            }
            return false;
        }

        private IEnumerator HalveDamage(Hit hit)
        {
            hit.damage -= Mathf.CeilToInt(hit.damage / 2f);
            yield break;
        }

        public override bool RunPreCardPlayedEvent(Entity entity, Entity[] targets)
        {
            if (toClear == 0 && entity == target && count > 0 && targets != null && targets.Length > 0)
            {
                toClear = 1;
            }

            return false;
        }

        public override bool RunActionPerformedEvent(PlayAction action)
        {
            if (toClear > 0)
            {
                return ActionQueue.Empty;
            }

            return false;
        }

        public IEnumerator ActionPerformed(PlayAction action)
        {
            toClear = 0;
            yield return Clear(toClear);
        }

        public IEnumerator Clear(int amount)
        {
            Events.InvokeStatusEffectCountDown(this, ref amount);
            if (amount != 0)
            {
                yield return CountDown(target, amount);
            }
        }
    }

    public class StatusEffectSTSAmplify : StatusEffectData
    {
        public bool cardPlayed;

        public int current;

        public int amountToClear;

        public override void Init()
        {
            base.OnActionPerformed += ActionPerformed;
        }

        public override bool RunStackEvent(int stacks)
        {
            current += stacks;
            target.effectBonus += stacks;
            target.PromptUpdate();
            return false;
        }

        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            if (!cardPlayed && entity == target && count > 0 && targets != null && targets.Length != 0)
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
            int amount2 = amount;
            Events.InvokeStatusEffectCountDown(this, ref amount2);
            if (amount2 != 0)
            {
                current -= amount2;
                target.effectBonus -= amount2;
                target.PromptUpdate();
                yield return CountDown(target, amount2);
            }
        }

        public override bool RunEndEvent()
        {
            target.effectBonus -= current;
            target.PromptUpdate();
            return false;
        }
    }

    public class StatusEffectSTSFlight : StatusEffectData
    {
        public override void Init()
        {
            base.OnHit += FlightHit;
        }

        public override bool RunHitEvent(Hit hit)
        {
            if (hit.Offensive && count > 0 && hit.damage > 0)
            {
                return hit.target == target;
            }

            return false;
        }

        public IEnumerator FlightHit(Hit hit)
        {
            hit.damage = Mathf.RoundToInt(hit.damage / 2f);
            ActionQueue.Stack(new ActionSequence(CountDown())
            {
                fixedPosition = true,
                note = "Count Down Flight"
            });
            yield break;
        }

        public IEnumerator CountDown()
        {
            if ((bool)this && (bool)target && target.alive)
            {
                int amount = 1;
                Events.InvokeStatusEffectCountDown(this, ref amount);
                if (amount != 0)
                {
                    yield return CountDown(target, amount);
                }
            }
        }
    }

    public class StatusEffectSTSMark : StatusEffectData
    {
        public override void Init()
        {
            base.OnStack += DoStuff;
        }

        private IEnumerator DoStuff(int stacks)
        {
            // All enemies with Mark lose hp
            foreach (Entity entity in Battle.GetAllUnits(Battle.GetOpponent(GetDamager().owner)))
            {
                foreach (StatusEffectData effect in entity.statusEffects)
                {
                    if (effect is StatusEffectSTSMark && effect.count > 0)
                    {
                        // Hit em
                        Hit hit = new Hit(GetDamager(), entity, effect.count)
                        {
                            screenShake = 0.25f,
                            canRetaliate = false,
                        };
                        // Add VFX Later?
                        //var transform = entity.transform;
                        //VFXMod.instance.VFX.TryPlayEffect("stsmark", transform.position, transform.lossyScale);
                        // Add SFX Later?
                        //VFXMod.instance.SFX.TryPlaySound("stsmark");
                        yield return hit.Process();
                        yield return Sequences.Wait(0.2f);
                    }
                }
            }
        }
    }

    public class StatusEffectApplyXWhenHitOnce : StatusEffectApplyXWhenHit
    {
        public override void Init()
        {
            base.PostHit += RemoveMe;
        }

        public IEnumerator RemoveMe(Hit hit)
        {
            yield return Run(GetTargets(hit, GetTargetContainers(), GetTargetActualContainers()), hit.damage + hit.damageBlocked);
            ActionQueue.Stack(new ActionSequence(CountDown())
            {
                fixedPosition = true,
                note = "Remove Apply When Hit Once"
            });
            yield break;
        }

        public IEnumerator CountDown()
        {
            if ((bool)this && (bool)target && target.alive)
            {
                int amount = GetAmount();
                Events.InvokeStatusEffectCountDown(this, ref amount);
                if (amount != 0)
                {
                    yield return CountDown(target, amount);
                }
            }
        }
    }

    public class StatusEffectApplyXToFrontEnemies : StatusEffectApplyX
    {
        public override void Init()
        {
            base.OnCardPlayed += Run;
        }

        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            return entity == target;
        }

        public IEnumerator Run(Entity entity, Entity[] targets)
        {
            int a = GetAmount();
            List<Entity> toAffect = new List<Entity>();
            foreach (CardContainer row in Battle.instance.GetRows(Battle.GetOpponent(target.owner)))
            {
                toAffect.AddIfNotNull(row.GetTop());
            }

            if (toAffect.Count <= 0)
            {
                yield break;
            }

            target.curveAnimator.Ping();
            yield return Sequences.Wait(0.13f);
            Routine.Clump clump = new Routine.Clump();
            foreach (Entity item in toAffect)
            {
                Hit hit = new Hit(target, item, 0);
                hit.AddStatusEffect(effectToApply, a);
                clump.Add(hit.Process());
            }

            yield return clump.WaitForEnd();
            yield return Sequences.Wait(0.13f);
        }
    }

    public class StatusEffectApplyXToFrontEnemiesWhenHit : StatusEffectApplyX
    {
        [SerializeField]
        public TargetConstraint[] attackerConstraints;

        public override void Init()
        {
            base.PostHit += CheckHit;
        }

        public override bool RunPostHitEvent(Hit hit)
        {
            if (target.enabled && hit.target == target && hit.canRetaliate && (!targetMustBeAlive || (target.alive && Battle.IsOnBoard(target))) && hit.Offensive && hit.BasicHit)
            {
                return CheckAttackerConstraints(hit.attacker);
            }

            return false;
        }

        public IEnumerator CheckHit(Hit hit)
        {
            List<Entity> toAffect = new List<Entity>();
            foreach (CardContainer row in Battle.instance.GetRows(Battle.GetOpponent(target.owner)))
            {
                toAffect.AddIfNotNull(row.GetTop());
            }
            return Run(toAffect);
        }

        public bool CheckAttackerConstraints(Entity attacker)
        {
            if (attackerConstraints != null)
            {
                TargetConstraint[] array = attackerConstraints;
                for (int i = 0; i < array.Length; i++)
                {
                    if (!array[i].Check(attacker))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    public class StatusEffectInstantIncreaseCounter : StatusEffectInstant
    {
        public override IEnumerator Process()
        {
            target.counter.current += GetAmount();
            target.PromptUpdate();
            yield return base.Process();
        }
    }

    public class StatusEffectApplyXWhenAnyCardIsPlayed : StatusEffectApplyX
    {
        public TargetConstraint[] triggerConstraints;
        public Boolean targetPlayedCard;

        public override void Init()
        {
            base.OnCardPlayed += Check;
        }

        public static CardContainer[] GetWasInRows(Entity entity, IEnumerable<Entity> targets)
        {
            if (entity.data.playType == Card.PlayType.Play && entity.NeedsTarget)
            {
                HashSet<CardContainer> list = new HashSet<CardContainer>();
                foreach (Entity target in targets)
                {
                    if (target.containers != null && target.containers.Length != 0)
                    {
                        list.AddRange(target.containers);
                    }
                    else
                    {
                        list.AddRange(target.preContainers);
                    }
                }

                return list.ToArray();
            }

            return entity.containers;
        }

        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            if (target.enabled)
            {
                foreach (TargetConstraint triggerConstraint in triggerConstraints)
                {
                    if (!triggerConstraint.Check(entity))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public IEnumerator Check(Entity entity, Entity[] targets)
        {
            if (targetPlayedCard)
            {
                return Run(new List<Entity>() { entity });
            }
            else
            {
                return Run(GetTargets(null, GetWasInRows(entity, targets), null, targets));
            }
        }
    }
}
