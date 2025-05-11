using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenAllyAttacksCountDown : SpirefrostBuilder
    {
        internal static string ID => "When Ally Attacks Count Down";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXWhenAlliesAttackOnBoard>(ID)
                .WithText("When an ally attacks, count down <keyword=counter> by <{a}>")
                .WithCanBeBoosted(true)
                .WithStackable(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenAlliesAttackOnBoard>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("Reduce Counter");
                });
        }
    }
}
