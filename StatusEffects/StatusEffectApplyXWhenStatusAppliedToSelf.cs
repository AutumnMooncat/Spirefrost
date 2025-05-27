using Spirefrost.Patches;
using System.Collections;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectApplyXWhenStatusAppliedToSelf : StatusEffectApplyX
    {
        public bool selfCanBeApplier;

        public bool allyCanBeApplier = true;

        public bool enemyCanBeApplier = true;

        public bool itemCanBeApplier = true;

        public bool anyStatus = true;

        public string[] statusTypes;

        public override void Init()
        {
            base.PostApplyStatus += DoApply;
        }

        public override bool RunPostApplyStatusEvent(StatusEffectApply apply)
        {
            if (target.enabled && ShouldApply(apply.effectData) && !StatusSystemPatch.isTemp)
            {
                return apply.target == target && ApplierCheck(apply.applier);
            }

            return false;
        }

        private bool ApplierCheck(Entity applier)
        {
            if (applier == target)
            {
                return selfCanBeApplier;
            }

            if (applier.data.IsItem)
            {
                return itemCanBeApplier;
            }

            if (applier.owner == target.owner)
            {
                return allyCanBeApplier;
            }
            else
            {
                return enemyCanBeApplier;
            }
        }

        private bool ShouldApply(StatusEffectData apply)
        {
            return apply != null && apply.isStatus && apply.visible && (anyStatus || statusTypes.Contains(apply.type));
        }

        public IEnumerator DoApply(StatusEffectApply apply)
        {
            return Run(GetTargets());
        }
    }
}
