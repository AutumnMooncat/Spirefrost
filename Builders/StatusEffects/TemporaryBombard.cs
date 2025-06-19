using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class TemporaryBombard : SpirefrostBuilder
    {
        internal static string ID => "Temporary Bombard";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Temporary Barrage", ID)
                .SubscribeToAfterAllBuildEvent<StatusEffectTemporaryTrait>(data =>
                {
                    data.trait = TryGet<TraitData>("Bombard 1");
                    data.targetConstraints = BombardConstraints();
                });
        }

        internal static TargetConstraint[] BombardConstraints()
        {
            return new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintIsUnit>(),
                        MakeConstraint<TargetConstraintDoesDamage>(),
                        MakeConstraint<TargetConstraintMaxCounterMoreThan>(),
                        MakeConstraint<TargetConstraintPlayOnSlot>(c =>
                        {
                            c.board = true;
                        }),
                        MakeConstraint<TargetConstraintPlayOnSlot>(c =>
                        {
                            c.slot = true;
                            c.not = true;
                        }),
                        MakeConstraint<TargetConstraintHasTrait>(c => {
                            c.trait = TryGet<TraitData>("Bombard 1");
                            c.not = true;
                        }),
                        MakeConstraint<TargetConstraintHasTrait>(c => {
                            c.trait = TryGet<TraitData>("Barrage");
                            c.not = true;
                        }),
                        MakeConstraint<TargetConstraintHasTrait>(c => {
                            c.trait = TryGet<TraitData>("Aimless");
                            c.not = true;
                        }),
                        MakeConstraint<TargetConstraintHasTrait>(c => {
                            c.trait = TryGet<TraitData>("Longshot");
                            c.not = true;
                        }),
                        MakeConstraint<TargetConstraintNeedsTarget>()
                    };
        }
    }

    internal class InstantGainBombard : SpirefrostBuilder
    {
        internal static string ID => "Instant Gain Bombard";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Instant Gain Aimless", ID)
                .WithText("Add <keyword=bombard> to the target")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXInstant>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(TemporaryBombard.ID);
                    data.targetConstraints = TemporaryBombard.BombardConstraints();
                });
        }
    }
}
