using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenHitGainFury : SpirefrostBuilder
    {
        internal static string ID => "When Hit Gain Fury";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Hit Gain Teeth To Self", ID)
                .WithText("When hit, gain <keyword=fury {a}>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenHit>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Temporary Fury");
                });
        }
    }
}
