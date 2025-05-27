using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class TriggerWhenShellAppliedToSelf : SpirefrostBuilder
    {
        internal static string ID => "Trigger When Shell Applied To Self";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXWhenYAppliedToSelf>(ID)
                .WithText("Trigger when <keyword=shell>'d")
                .WithIsReaction(true)
                .WithCanBeBoosted(false)
                .WithStackable(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenYAppliedToSelf>(data =>
                {
                    data.textOrder = 1;
                    data.whenAppliedType = TryGet<StatusEffectData>("Shell").type;
                    data.whenAppliedTypes = new string[]
                    {
                        TryGet<StatusEffectData>("Shell").type
                    };
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
                    data.eventPriority = -99;
                    data.descColorHex = "F99C61";
                });
        }
    }
}
