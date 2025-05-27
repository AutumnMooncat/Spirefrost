using System.Collections;
using UnityEngine;

namespace Spirefrost.StatusEffects
{
    public class StatusEffectSTSFlight : StatusEffectData
    {
        public override void Init()
        {
            OnHit += FlightHit;
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
}
