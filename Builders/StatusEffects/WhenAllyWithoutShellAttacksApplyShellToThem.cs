using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenAllyWithoutShellAttacksApplyShellToThem : SpirefrostBuilder
    {
        internal static string ID => "When Ally Without Shell Attacks Apply Shell To Them";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXWhenAlliesAttackOnBoard>(ID)
                .WithText("When an ally without <keyword=shell> attacks, apply <{a}><keyword=shell> to them")
                .WithCanBeBoosted(true)
                .WithStackable(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenAlliesAttackOnBoard>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Attacker;
                    data.effectToApply = TryGet<StatusEffectData>("Shell");
                    data.applyConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintHasStatus>(t =>
                        {
                            t.status = TryGet<StatusEffectData>("Shell");
                            t.not = true;
                        })
                    };
                });
        }
    }
}
