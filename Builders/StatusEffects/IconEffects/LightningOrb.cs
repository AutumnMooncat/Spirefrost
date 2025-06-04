using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using Spirefrost.StatusEffects;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.StatusEffects.IconEffects
{
    internal class LightningOrb : SpirefrostBuilder
    {
        internal static string ID => "STS Lightning";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static int ApplyAmount => 1;

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectOrb>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(false)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectOrb>(data =>
                {
                    data.passiveEffect = TryGet<StatusEffectData>(LightningOrbDamage.ID);
                    data.passiveFlags = StatusEffectApplyX.ApplyToFlags.RandomEnemy;
                    data.evokeFactor = 2f;
                    data.evokeEffect = TryGet<StatusEffectData>(LightningOrbDamage.ID);
                    data.evokeFlags = StatusEffectApplyX.ApplyToFlags.RandomEnemy;
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
                .Subscribe_WithStatusIcon(LightningIcon.ID);
        }
    }

    internal class LightningOrbDamage : SpirefrostBuilder
    {
        internal static string ID => "Lightning Orb Damage";

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
                    data.damageType = LightningIcon.DamageID;
                });
        }
    }
}
