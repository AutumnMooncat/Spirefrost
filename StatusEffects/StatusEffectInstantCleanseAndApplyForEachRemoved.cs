using System.Collections;
using System.Linq;

namespace Spirefrost.StatusEffects
{
    public class StatusEffectInstantCleanseAndApplyForEachRemoved : StatusEffectInstantCleanse
    {
        public StatusEffectData effectToApply;

        public override IEnumerator Process()
        {
            int num = target.statusEffects.Select(status => status.IsNegativeStatusEffect() ? 1 : 0).Sum();
            yield return base.Process();
            if (num > 0)
            {
                yield return StatusEffectSystem.Apply(target, applier, effectToApply, num * GetAmount());
            }
        }
    }
}
