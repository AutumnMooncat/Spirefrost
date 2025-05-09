using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenSpiceAppliedToSelfShellRandomAlly : SpirefrostBuilder
    {
        internal static string ID => "When Spice Applied To Self Shell Random Ally";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Snow Applied To Self Gain Equal Attack", ID)
                .WithText("When <keyword=spice>'d, apply equal <keyword=shell> to a random ally")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenYAppliedTo>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomAlly;
                    data.effectToApply = TryGet<StatusEffectData>("Shell");
                    data.whenAppliedTypes = new string[]
                    {
                        "spice"
                    };
                });
        }
    }
}
