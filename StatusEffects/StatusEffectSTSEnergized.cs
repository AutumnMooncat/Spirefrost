using System.Collections;
using UnityEngine;

namespace Spirefrost.StatusEffects
{
    public class StatusEffectSTSEnergized : StatusEffectData
    {
        public override void Init()
        {
            SpirefrostEvents.OnCounterReset += CounterTrigger;
        }

        public void OnDestroy()
        {
            SpirefrostEvents.OnCounterReset -= CounterTrigger;
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

        private IEnumerator CounterTrigger(Entity entity)
        {
            Hit hit = new Hit(applier, target, 0)
            {
                countsAsHit = false,
                counterReduction = GetAmount()
            };
            yield return hit.Process();
            yield return CountDown();
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
}
