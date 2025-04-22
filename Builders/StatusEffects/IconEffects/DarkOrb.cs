using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.StatusEffects.IconEffects
{
    internal class DarkOrb : SpirefrostBuilder
    {
        internal static string ID => "STS Dark";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static int ApplyAmount => 0;

        internal static int ScaleAmount => 1;

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectOrb>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(false)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectOrb>(data =>
                {
                    data.perTurnIncrease = ScaleAmount;
                    data.triggerOnRemove = true;
                    data.dealDamage = true;
                    data.doesDamage = true;
                    data.countsAsHit = true;
                    data.canRetaliate = false;
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Enemies;
                    data.applyConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintPseudoBarrage>()
                    };
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
                })
                .Subscribe_WithStatusIcon(DarkIcon.ID);
        }
    }
}
