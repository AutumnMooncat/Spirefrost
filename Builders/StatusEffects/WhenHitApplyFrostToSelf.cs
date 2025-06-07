using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenHitApplyFrostToSelf : SpirefrostBuilder
    {
        internal static string ID => "When Hit Apply Frost To Self";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Hit Apply Spice To Self", ID)
                .WithText("When hit, gain <{a}><keyword=frost>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenHit>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Frost");
                });
        }
    }
}
