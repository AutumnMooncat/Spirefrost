using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class TemporaryDraw : SpirefrostBuilder
    {
        internal static string ID => "Temporary Draw";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Temporary Fury", ID)
                .WithCanBeBoosted(true)
                .WithStackable(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectTemporaryTrait>(data =>
                {
                    data.trait = TryGet<TraitData>("Draw");
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintOr>(or =>
                        {
                            or.constraints = new TargetConstraint[]
                            {
                                MakeConstraint<TargetConstraintMaxCounterMoreThan>(c => c.moreThan = 0),
                                MakeConstraint<TargetConstraintHasReaction>()
                            };
                        })
                    };
                });
        }
    }

    internal class InstantApplyTemporaryDrawToAlly : SpirefrostBuilder
    {
        internal static string ID => "Instant Apply Temporary Draw To Ally";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Instant Gain Fury", ID)
                .WithText($"Add <keyword=draw {{a}}> to an ally")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXInstant>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(TemporaryDraw.ID);
                    data.applyConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintOr>(or =>
                        {
                            or.constraints = new TargetConstraint[]
                            {
                                MakeConstraint<TargetConstraintMaxCounterMoreThan>(c => c.moreThan = 0),
                                MakeConstraint<TargetConstraintHasReaction>()
                            };
                        })
                    };
                });
        }
    }
}
