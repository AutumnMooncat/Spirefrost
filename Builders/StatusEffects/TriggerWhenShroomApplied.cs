using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class TriggerWhenShroomApplied : SpirefrostBuilder
    {
        internal static string ID => "Trigger When Shroom Applied";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Trigger Against When Snow Applied", ID)
                .WithText("Trigger when anything is hit with {e}")
                .WithTextInsert("<keyword=shroom>")
                .SubscribeToAfterAllBuildEvent<StatusEffectTriggerWhenStatusApplied>(data =>
                {
                    data.textOrder = 1;
                    data.targetStatus = TryGet<StatusEffectData>("Shroom");
                    data.triggerType = StatusEffectTriggerWhenStatusApplied.TriggerType.Normal;
                    data.WithSwappable(TryGet<StatusEffectData>("MultiHit"));
                });
        }
    }
}
