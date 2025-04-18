using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenHealedReduceCounter : SpirefrostBuilder
    {
        internal static string ID => "When Healed Reduce Counter";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Healed Apply Attack To Self", ID)
                .WithText("When healed, count down <keyword=counter> by <{a}>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenHealed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Reduce Counter");
                });
        }
    }
}
