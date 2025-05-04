using System.Collections;

namespace Spirefrost
{
    public class StatusEffectJudgement : StatusEffectInstant
    {
        public override IEnumerator Process()
        {
            if (target.hp.current <= GetAmount())
            {
                target.hp.current = 0;
                target.ResetWhenHealthLostEffects();
                target.PromptUpdate();
            }
            return base.Process();
        }
    }
}
