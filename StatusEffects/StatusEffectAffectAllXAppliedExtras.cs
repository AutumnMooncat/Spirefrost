namespace Spirefrost
{
    public class StatusEffectAffectAllXAppliedExtras : StatusEffectAffectAllXApplied
    {
        public enum LocationRequirement
        {
            None,
            Hand,
            Board,
        }

        public bool applierMustBeSelf;

        public ScriptableAmount scriptableAmount;

        public LocationRequirement location = LocationRequirement.None;

        public override bool RunApplyStatusEvent(StatusEffectApply apply)
        {
            if (applierMustBeSelf && apply.applier != target)
            {
                return false;
            }
            if (location == LocationRequirement.Hand && !References.Player.handContainer.Contains(target))
            {
                return false;
            }
            if (location == LocationRequirement.Board && !Battle.IsOnBoard(target))
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
