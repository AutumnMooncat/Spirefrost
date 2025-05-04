using System.Collections;
using UnityEngine;

namespace Spirefrost
{
    public class StatusEffectSTSWeakness : StatusEffectData
    {
        public bool cardPlayed;

        public StatusEffectSTSWeakness()
        {
            eventPriority = 1;
            removeOnDiscard = true;
        }

        public override void Init()
        {
            base.OnActionPerformed += ActionPerformed;
            base.OnHit += HalveDamage;
        }

        public override bool RunHitEvent(Hit hit)
        {
            if (hit.attacker == target && hit.countsAsHit && hit.damage > 0)
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

        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            if (!cardPlayed && entity == target && count > 0)
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
            yield return Clear(1);
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
}
