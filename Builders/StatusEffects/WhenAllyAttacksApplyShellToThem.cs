using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenAllyAttacksApplyShellToThem : SpirefrostBuilder
    {
        internal static string ID => null;

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXWhenAlliesAttackOnBoard>(ID)
                .WithText("When an ally attacks, apply <{a}><keyword=shell> to them")
                .WithCanBeBoosted(true)
                .WithStackable(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenAlliesAttackOnBoard>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Attacker;
                    data.effectToApply = TryGet<StatusEffectData>("Shell");
                });
        }
    }
}
