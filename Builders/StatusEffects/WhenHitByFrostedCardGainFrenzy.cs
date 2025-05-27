using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenHitByFrostedCardGainFrenzy : SpirefrostBuilder
    {
        internal static string ID => "When Hit By Frosted Card Gain Frenzy";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Hit With Junk Add Frenzy To Self", ID)
                .WithText("When hit by a <keyword=frost>'d card, gain <x{a}><keyword=frenzy>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenHit>(data =>
                {
                    data.attackerConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintHasStatus>(c =>
                        {
                            c.status = TryGet<StatusEffectData>("Frost");
                        })
                    };
                });
        }
    }
}
