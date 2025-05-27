using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class TriggerWhenSpiceAppliedToSelf : SpirefrostBuilder
    {
        internal static string ID => "Trigger When Spice Applied To Self";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXWhenStatusAppliedToSelf>(ID)
                .WithText("Trigger when <keyword=spice>'d")
                .WithIsReaction(true)
                .WithCanBeBoosted(false)
                .WithStackable(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenStatusAppliedToSelf>(data =>
                {
                    data.textOrder = 1;
                    data.anyStatus = false;
                    data.statusTypes = new string[]
                    {
                        "spice"
                    };
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
                    data.eventPriority = -99;
                    data.descColorHex = "F99C61";
                });
        }
    }
}
