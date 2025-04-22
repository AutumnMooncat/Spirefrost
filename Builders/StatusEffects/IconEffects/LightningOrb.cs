using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using UnityEngine;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.StatusEffects.IconEffects
{
    internal class LightningOrb : SpirefrostBuilder
    {
        internal static string ID => "STS Lightning";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static int ApplyAmount => 2;

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectOrb>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(false)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectOrb>(data =>
                {
                    data.dealDamage = true;
                    data.doesDamage = true;
                    data.countsAsHit = true;
                    data.canRetaliate = false;
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomEnemy;
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
}
