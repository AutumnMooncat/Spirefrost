using System.Collections;

namespace Spirefrost
{
    public class StatusEffectInstantChannel : StatusEffectInstant
    {
        public StatusEffectOrb orbToChannel;

        public int orbAmount;

        public override IEnumerator Process()
        {
            for (int i = 0; i < GetAmount(); i++)
            {
                yield return StatusEffectSystem.Apply(target, applier, orbToChannel, orbAmount);
            }
            yield return base.Process();
        }
    }
}
