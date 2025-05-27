using System.Collections;

namespace Spirefrost.StatusEffects
{
    public class StatusEffectInstantIncreaseMaxCounterAdjusted : StatusEffectInstant
    {
        public override IEnumerator Process()
        {
            if (target.counter.current > 0)
            {
                target.counter.current += GetAmount();
            }
            target.counter.max += GetAmount();
            target.PromptUpdate();
            yield return base.Process();
        }
    }
}
