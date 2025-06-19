using System.Collections;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectInstantRunnable : StatusEffectInstant
    {
        public delegate IEnumerator Runnable(StatusEffectData data);

        public Runnable runnable;

        public override IEnumerator Process()
        {
            Runnable toRun = runnable ?? ((StatusEffectInstantRunnable)original).runnable;
            ActionQueue.Stack(new ActionSequence(toRun.Invoke(this))
            {
                fixedPosition = true,
                note = "Instant Runnable"
            });
            yield return base.Process();
        }
    }
}
