namespace Spirefrost
{
    public class StatusEffectAffectAllXAppliedWhileInHand : StatusEffectAffectAllXApplied
    {
        public bool applierMustBeSelf;

        public ScriptableAmount scriptableAmount;

        public override bool RunApplyStatusEvent(StatusEffectApply apply)
        {
            if (applierMustBeSelf && apply.applier != target)
            {
                return false;
            }
            if (!References.Player.handContainer.Contains(target))
            {
                return false;
            }
            if (scriptableAmount != null)
            {
                apply.count += scriptableAmount.Get(target);
                return false;
            }
            return base.RunApplyStatusEvent(apply);
        }
    }
}
