using System;
using System.Collections;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectSTSIntangible : StatusEffectData
    {
        public override void Init()
        {
            base.OnHit += Check;
        }

        public override bool RunBeginEvent()
        {
            target.cannotBeHitCount++;
            return false;
        }

        public override bool RunEndEvent()
        {
            target.cannotBeHitCount--;
            return false;
        }

        public override bool RunHitEvent(Hit hit)
        {
            if (hit.target == target)
            {
                return hit.damage > 0;
            }

            return false;
        }

        public IEnumerator Check(Hit hit)
        {
            int reduction = Math.Min(hit.damage, count);
            count -= reduction;
            hit.damage -= reduction;
            hit.damageBlocked += reduction;

            if (count <= 0)
            {
                yield return Remove();
            }

            target.PromptUpdate();
        }
    }
}
