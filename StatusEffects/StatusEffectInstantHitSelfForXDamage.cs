using System.Collections;
using UnityEngine;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectInstantHitSelfForXDamage : StatusEffectInstant
    {
        public override IEnumerator Process()
        {
            float num = target.curveAnimator.Ping();
            yield return Sequences.Wait(num);

            Hit hit = new Hit(target, target, GetAmount());
            hit.AddAttackerStatuses();
            yield return StatusEffectSystem.PreAttackEvent(hit);
            yield return hit.Process();
            yield return StatusEffectSystem.PostAttackEvent(hit);
            yield return base.Process();
        }
    }
}
