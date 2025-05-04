using System.Collections;

namespace Spirefrost
{
    public class StatusEffectInstantLoseHealth : StatusEffectInstant
    {
        public override IEnumerator Process()
        {
            target.hp.current -= GetAmount();
            target.ResetWhenHealthLostEffects();
            target.PromptUpdate();
            return base.Process();
        }
    }
}
