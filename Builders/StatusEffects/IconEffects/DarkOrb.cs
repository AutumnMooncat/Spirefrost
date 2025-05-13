using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using Spirefrost.StatusEffects;
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
                    data.passiveIncrease = ScaleAmount;
                    data.evokeEffect = TryGet<StatusEffectData>(DarkOrbDamage.ID);
                    data.evokeFlags = StatusEffectApplyX.ApplyToFlags.Enemies;
                    data.evokeApplyConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintPseudoBarrage>(t => t.doesDamage = true)
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

    internal class DarkOrbDamage : SpirefrostBuilder
    {
        internal static string ID => "Dark Orb Damage";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectInstantDamage>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantDamage>(data =>
                {
                    data.doesDamage = true;
                    data.countsAsHit = true;
                    data.canRetaliate = false;
                });
        }
    }
}
