using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.Traits;

namespace Spirefrost.Builders.StatusEffects
{
    internal class TemporaryFocus : SpirefrostBuilder
    {
        internal static string ID => "Temporary Focus";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Temporary Pigheaded", ID)
                .WithCanBeBoosted(true)
                .WithStackable(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectTemporaryTrait>(data =>
                {
                    data.trait = TryGet<TraitData>(FocusTrait.ID);
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintIsUnit>()
                    };
                });
        }
    }

    internal class InstantApplyTemporaryFocus : SpirefrostBuilder
    {
        internal static string ID => "Instant Apply Temporary Focus";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Instant Gain Aimless", ID)
                .WithText($"Add <keyword={FocusKeyword.FullID} {{a}}> to the target")
                .WithCanBeBoosted(true)
                .WithStackable(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXInstant>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(TemporaryFocus.ID);
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintIsUnit>()
                    };
                });
        }
    }
}
