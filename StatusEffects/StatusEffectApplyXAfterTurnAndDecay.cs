using System.Collections;
using UnityEngine;

namespace Spirefrost.StatusEffects
{
    public class StatusEffectApplyXAfterTurnAndDecay : StatusEffectApplyX
    {
        public bool subbed;

        public bool primed;

        public bool ignoreSilence = true;

        public override void Init()
        {
            OnTurnEnd += PostTurn;
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
            if (ignoreSilence)
            {
                return false;
            }
            return base.TargetSilenced();
        }

        public override int GetAmount()
        {
            if (!target || target.silenced && !ignoreSilence)
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
            if (primed && target.enabled && Battle.IsOnBoard(target))
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
}
