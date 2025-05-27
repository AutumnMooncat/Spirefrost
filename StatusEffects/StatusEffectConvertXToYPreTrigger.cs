using System.Collections;
using UnityEngine;

namespace Spirefrost.StatusEffects
{
    public class StatusEffectConvertXToYPreTrigger : StatusEffectApplyXPreTrigger
    {
        public enum Conversion
        {
            Health,
            Attack,
            Scrap,
        }

        public Conversion toConvert;

        private int oldTemp;

        private bool cardPlayed;

        public StatusEffectConvertXToYPreTrigger()
        {
            scriptableAmount = CreateInstance<ScriptableFixedAmount>();
            ((ScriptableFixedAmount)scriptableAmount).amount = 0;
            eventPriority = -10000;
        }

        public override void Init()
        {
            PreTrigger += EntityPreTrigger;
            OnActionPerformed += ActionPerformed;
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
            switch (toConvert)
            {
                case Conversion.Health:
                    if (target.hp.current > reduceTo)
                    {
                        ((ScriptableFixedAmount)scriptableAmount).amount = target.hp.current - reduceTo;
                        target.hp.current = reduceTo;
                        prompt = true;
                    }

                    if (target.hp.max > reduceTo)
                    {
                        target.hp.max = reduceTo;
                        prompt = true;
                    }
                    break;
                case Conversion.Attack:
                    if (target.damage.current + target.tempDamage.Value > reduceTo)
                    {
                        // Current damage goes to reduceTo if higher than it
                        // Temp damage needs to be set so damage+temp = reduceTo
                        // Restore temp later so it doesnt mess stuff up
                        ((ScriptableFixedAmount)scriptableAmount).amount = target.damage.current + target.tempDamage.Value - reduceTo;
                        if (target.damage.current > reduceTo)
                        {
                            target.damage.current = reduceTo;
                            prompt = true;
                        }

                        oldTemp = target.tempDamage.Value;
                        target.tempDamage.Value = reduceTo - target.damage.current;
                    }

                    // Also reduce max if applicable
                    if (target.damage.max > reduceTo)
                    {
                        target.damage.max = reduceTo;
                        prompt = true;
                    }
                    break;
                case Conversion.Scrap:
                    StatusEffectData scrapEffect = target.statusEffects.Find((a) => a.name.Equals("Scrap"));
                    if (scrapEffect?.count > reduceTo)
                    {
                        if (doPing)
                        {
                            target.curveAnimator.Ping();
                        }
                        count = scrapEffect.count - 1;
                        yield return scrapEffect.RemoveStacks(scrapEffect.count - reduceTo, removeTemporary: false);
                    }
                    break;
            }
            if (prompt)
            {
                target.PromptUpdate();
            }
            if (((ScriptableFixedAmount)scriptableAmount).amount > 0)
            {
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
            cardPlayed = false;
            target.tempDamage.Value += oldTemp;
            oldTemp = 0;
            yield break;
        }
    }
}
