using System.Collections;
using UnityEngine;

namespace Spirefrost
{
    public class StatusEffectOrb : StatusEffectApplyX
    {
        public bool cardPlayed;

        public bool subbed;

        public bool primed;

        public int perTurnIncrease;

        public bool triggerOnRemove;

        public override void Init()
        {
            base.OnTurnEnd += PostTurn;
            base.OnActionPerformed += ActionPerformed;
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
            if (primed && target.enabled && Battle.IsOnBoard(target))
            {
                return entity == target;
            }

            return false;
        }

        public IEnumerator PostTurn(Entity entity)
        {
            if (!triggerOnRemove)
            {
                //Debug.Log($"Passive effect for {this} triggered");
                yield return Run(GetTargets());
            }
            if (perTurnIncrease != 0)
            {
                //Debug.Log($"Scale effect for {this} triggered");
                count += perTurnIncrease;
                target.PromptUpdate();
            }
        }

        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            if (!cardPlayed && entity == target)
            {
                //Debug.Log($"{target} with {this} played, primed to remove");
                cardPlayed = true;
            }

            return false;
        }

        public override bool RunActionPerformedEvent(PlayAction action)
        {
            if (cardPlayed)
            {
                //Debug.Log($"{target} with {this} performed action, can we remove yet? {ActionQueue.Empty}");
                return ActionQueue.Empty;
            }

            return false;
        }

        public IEnumerator ActionPerformed(PlayAction action)
        {
            cardPlayed = false;
            if (triggerOnRemove)
            {
                //Debug.Log($"Triggering evoke effect for {this}");
                yield return Run(GetTargets());
            }
            //Debug.Log($"Calling clear for {this}");
            yield return Clear(count);
        }

        public IEnumerator Clear(int amount)
        {
            //Debug.Log($"Clear {amount} called for {this}");
            // Still remove Dark at 0
            if (amount == 0) { amount++; }
            Events.InvokeStatusEffectCountDown(this, ref amount);
            if (amount != 0)
            {
                //Debug.Log($"Counting Down {this} by {amount}, it currently has {count} stacks");
                yield return CountDown(target, amount);
            }
        }
    }
}
