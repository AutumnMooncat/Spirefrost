using System.Collections;
using UnityEngine;

namespace Spirefrost.StatusEffects
{
    public class StatusEffectInstantHalveHealthApplyEqualX : StatusEffectInstant
    {
        public StatusEffectData effectToApply;

        public bool equalToLostHealth;

        public override IEnumerator Process()
        {
            int lost = Mathf.FloorToInt(target.hp.current / 2f);
            if (lost > 1)
            {
                target.hp.current -= lost;
                target.ResetWhenHealthLostEffects();
                target.PromptUpdate();
                yield return StatusEffectSystem.Apply(target, applier, effectToApply, (equalToLostHealth ? lost : target.hp.current) * GetAmount());
            }
            yield return base.Process();
        }
    }
}
