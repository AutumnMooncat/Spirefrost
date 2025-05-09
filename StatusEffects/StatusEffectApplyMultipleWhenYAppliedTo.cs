using Deadpan.Enums.Engine.Components.Modding;
using System.Collections;
using static StatusEffectApplyX;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectApplyMultipleWhenYAppliedTo : StatusEffectApplyXWhenYAppliedTo
    {
        public class ApplyData
        {
            public StatusEffectData effect;

            public ApplyToFlags flags;

            public TargetConstraint[] constraints;
        }

        public ApplyData[] applyDatas;

        public override void Init()
        {
            base.PostApplyStatus += CompoundRun;
        }

        public IEnumerator CompoundRun(StatusEffectApply apply)
        {
            foreach (var item in applyDatas)
            {
                effectToApply = item.effect;
                applyToFlags = item.flags;
                applyConstraints = item.constraints;
                yield return Run(GetTargets(), apply.count);
            }
        }
    }
}
