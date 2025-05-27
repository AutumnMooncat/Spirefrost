using System.Collections;
using System.Linq;

namespace Spirefrost.StatusEffects
{
    public class StatusEffectInstantConvertStatusToX : StatusEffectInstant
    {
        public StatusEffectData effectToApply;

        public override IEnumerator Process()
        {
            int toApply = target.statusEffects.Select(status => status.isStatus && status.visible && !status.isReaction ? status.GetAmount() : 0).Sum();
            int num = target.statusEffects.Count;

            for (int i = num - 1; i >= 0; i--)
            {
                StatusEffectData statusEffectData = target.statusEffects[i];
                if (statusEffectData.isStatus && statusEffectData.visible && !statusEffectData.isReaction)
                {
                    yield return statusEffectData.Remove();
                }
            }

            target.PromptUpdate();

            if (toApply > 0)
            {
                yield return StatusEffectSystem.Apply(target, applier, effectToApply, toApply);
            }

            yield return base.Process();
        }
    }
}
