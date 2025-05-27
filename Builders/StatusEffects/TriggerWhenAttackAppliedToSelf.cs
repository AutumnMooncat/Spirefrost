using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class TriggerWhenAttackAppliedToSelf : SpirefrostBuilder
    {
        internal static string ID => "Trigger When Attack Applied To Self";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXWhenNonTempYAppliedToSelf>(ID)
                .WithText("Trigger when non-temporary <keyword=attack> gained")
                .WithIsReaction(true)
                .WithCanBeBoosted(false)
                .WithStackable(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenNonTempYAppliedToSelf>(data =>
                {
                    data.textOrder = 1;
                    data.whenAppliedType = TryGet<StatusEffectData>("Increase Attack").type;
                    data.whenAppliedTypes = new string[]
                    {
                        TryGet<StatusEffectData>("Increase Attack").type
                    };
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
                    data.eventPriority = -99;
                    data.descColorHex = "F99C61";
                });
        }
    }
}
