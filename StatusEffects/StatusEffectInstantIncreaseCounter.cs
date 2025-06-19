using System.Collections;

namespace Spirefrost.StatusEffects
{
    public class StatusEffectInstantIncreaseCounter : StatusEffectInstant
    {
        public override IEnumerator Process()
        {
            Hit hit = new Hit(applier, target, 0)
            {
                countsAsHit = false,
                counterReduction = -GetAmount()
            };
            yield return hit.Process();
            yield return base.Process();
        }
    }
}
