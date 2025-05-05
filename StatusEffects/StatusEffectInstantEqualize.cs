using System.Collections;
using System.Linq;

namespace Spirefrost
{
    public class StatusEffectInstantEqualize : StatusEffectInstant
    {
        public StatusEffectData[] effectsToEqualize;

        public override IEnumerator Process()
        {
            int max = 0;
            foreach (var status in target.statusEffects)
            {
                if (effectsToEqualize.Select(ete => ete.name).Contains(status.name))
                {
                    if (status.count > max)
                    {
                        max = status.count;
                    }
                }
            }
            foreach (var status in effectsToEqualize)
            {
                int toApply = max - target.statusEffects.Where(s => s.name == status.name).Select(s => s.count).FirstOrDefault();
                if (toApply > 0)
                {
                    yield return StatusEffectSystem.Apply(target, applier, status, toApply);
                }
            }
            yield return base.Process();
        }
    }
}
