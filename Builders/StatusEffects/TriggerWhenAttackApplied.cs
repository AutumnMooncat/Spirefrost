using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class TriggerWhenAttackApplied : SpirefrostBuilder
    {
        internal static string ID => "Trigger When Attack Applied";

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
                    data.whenAppliedType = TryGet<StatusEffectData>("Increase Attack").type;
                    data.whenAppliedTypes = new string[]
                    {
                        TryGet<StatusEffectData>("Increase Attack").type
                    };
                    data.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
                    data.eventPriority = -99;
                    data.descColorHex = "F99C61";
                });
        }
    }
}
