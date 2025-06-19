using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class InstantGainBarrage : SpirefrostBuilder
    {
        internal static string ID => "Instant Gain Barrage";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Instant Gain Aimless", ID)
                .WithText("Add <keyword=barrage> to the target")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXInstant>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Temporary Barrage");
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintHasTrait>(c => {
                            c.trait = TryGet<TraitData>("Barrage");
                            c.not = true;
                        })
                    };
                });
        }
    }
}
