using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Traits;
using Spirefrost.StatusEffects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
                    data.trait = TryGet<TraitData>(CustomBombardTrait.ID);
                    data.targetConstraints = BombardConstraints();
                });
        }

        internal static TargetConstraint[] BombardConstraints()
        {
            return new TargetConstraint[]
            {
                MakeConstraint<TargetConstraintIsUnit>(),
                MakeConstraint<TargetConstraintDoesDamage>(),
                MakeConstraint<TargetConstraintOr>(or =>
                {
                    or.constraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintMaxCounterMoreThan>(c => c.moreThan = 0),
                        MakeConstraint<TargetConstraintHasReaction>()
                    };
                }),
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
                    c.trait = TryGet<TraitData>(CustomBombardTrait.ID);
                    c.not = true;
                }),
                MakeConstraint<TargetConstraintHasTrait>(c => {
                    c.trait = TryGet<TraitData>("Bombard 1");
                    c.not = true;
                }),
                MakeConstraint<TargetConstraintHasTrait>(c => {
                    c.trait = TryGet<TraitData>("Bombard 2");
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

    internal class RunnableResetBombard : SpirefrostBuilder
    {
        internal static string ID => "Runnable Reset Bombard";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectInstantRunnable>(ID)
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantRunnable>(data =>
                {
                    data.runnable = Run;
                });
        }

        internal static IEnumerator Run(StatusEffectData data)
        {
            if (data.target.IsAliveAndExists())
            {
                // Avoid CME by storing all before calling
                List<StatusEffectBombard> found = new List<StatusEffectBombard>();
                foreach (var item in data.target.statusEffects)
                {
                    if (item is StatusEffectBombard bombard)
                    {
                        found.Add(bombard);
                    }
                }

                foreach (var item in found)
                {
                    yield return item.SetTargets();
                }
            }
        }
    }

    internal class InstantGainAndResetBombard : SpirefrostBuilder
    {
        internal static string ID => "Instant Gain And Reset Bombard";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectBetterInstantMultiple>(ID)
                .WithText("Add <keyword=bombard> to the target")
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectBetterInstantMultiple>(data =>
                {
                    data.effects = new StatusEffectData[]
                    {
                        TryGet<StatusEffectData>(InstantGainBombard.ID),
                        TryGet<StatusEffectData>(RunnableResetBombard.ID)
                    };
                    data.targetConstraints = TemporaryBombard.BombardConstraints();
                });
        }
    }
}
