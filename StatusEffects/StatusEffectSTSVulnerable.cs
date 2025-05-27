using System.Collections;
using UnityEngine;

namespace Spirefrost.StatusEffects
{
    public class StatusEffectSTSVulnerable : StatusEffectData
    {
        public bool cardPlayed;

        public StatusEffectSTSVulnerable()
        {
            removeOnDiscard = true;
        }

        public override void Init()
        {
            OnActionPerformed += ActionPerformed;
            OnHit += MultiplyHit;
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
            hit.damage = Mathf.CeilToInt(hit.damage * 1.5f);
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
