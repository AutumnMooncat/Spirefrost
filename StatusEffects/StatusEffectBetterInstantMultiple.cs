using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectBetterInstantMultiple : StatusEffectInstant
    {
        public StatusEffectData[] effects;

        public override bool CanStackActions => false;

        public override IEnumerator Process()
        {
            int amount = GetAmount();
            foreach (StatusEffectData statusEffectInstant in effects)
            {
                if (!statusEffectInstant.canBeBoosted || amount > 0)
                {
                    yield return StatusEffectSystem.Apply(target, applier, statusEffectInstant, amount, temporary: true);
                }
            }
            yield return base.Process();
        }
    }
}
