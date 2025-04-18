using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnHitIncreaseCounter : SpirefrostBuilder
    {
        internal static string ID => "On Hit Increase Counter";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Hit Pull Target", ID)
                .WithText("Count up target's <keyword=counter> by <{a}>")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnHit>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(IncreaseCounter.ID);
                });
        }
    }
}
