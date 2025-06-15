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

        private void CounterTrigger(Entity entity)
        {
            if (entity == target && target.alive)
            {
                IEnumerator logic = TriggerLogic();
                while (logic.MoveNext()) 
                {
                    object _ = logic.Current;
                }
            }
        }

        private IEnumerator TriggerLogic()
        {
            Routine.Clump clump = new Routine.Clump();
            Hit hit = new Hit(applier, target, 0)
            {
                countsAsHit = false,
                counterReduction = GetAmount()
            };
            clump.Add(hit.Process());
            clump.Add(CountDown());
            yield return clump.WaitForEnd();
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
