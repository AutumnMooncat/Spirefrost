using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class TemporaryFlourish : SpirefrostBuilder
    {
        internal static string ID => "Temporary Flourish";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Temporary Barrage", ID)
                .SubscribeToAfterAllBuildEvent<StatusEffectTemporaryTrait>(data =>
                {
                    data.trait = TryGet<TraitData>("Heartburn");
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintHasHealth>()
                    };
                });
        }
    }

    internal class InstantGainFlourish : SpirefrostBuilder
    {
        internal static string ID => "Instant Gain Flourish";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Instant Gain Aimless", ID)
                .WithText("Add <keyword=heartburn> to the target")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXInstant>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(TemporaryFlourish.ID);
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintHasHealth>(),
                        MakeConstraint<TargetConstraintHasTrait>(c => {
                            c.trait = TryGet<TraitData>("Heartburn");
                            c.not = true;
                        })
                    };
                });
        }
    }
}
