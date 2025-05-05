using System.Collections;
using UnityEngine;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectInstantDamage : StatusEffectInstant
    {
        public bool canRetaliate;

        public bool countsAsHit;

        public float factor = 1f;

        public override void Init()
        {
            base.Init();
            doesDamage = true;
        }

        public override IEnumerator Process()
        {
            Hit hit = new Hit(applier, target, Mathf.CeilToInt(GetAmount() * factor))
            {
                canRetaliate = canRetaliate,
                countsAsHit = countsAsHit
            };

            yield return hit.Process();
            yield return base.Process();
        }
    }
}
