using System.Collections;

namespace Spirefrost
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

        private void CounterTrigger(Entity entity)
        {
            if (entity == target && target.alive)
            {
                IEnumerator logic = TriggerLogic(entity);
                while (logic.MoveNext()) 
                {
                    object obj = logic.Current;
                }
            }
        }

        private IEnumerator TriggerLogic(Entity entity)
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
