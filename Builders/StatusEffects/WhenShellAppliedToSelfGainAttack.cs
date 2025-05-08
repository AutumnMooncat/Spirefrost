using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenShellAppliedToSelfGainAttack : SpirefrostBuilder
    {
        internal static string ID => "When Shell Applied To Self Gain Attack";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Snow Applied To Self Apply Demonize To Enemies", ID)
                .WithText("When <keyword=shell>'d, gain <+{a}><keyword=attack>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenYAppliedTo>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("Increase Attack");
                    data.whenAppliedTypes = new string[]
                    {
                        "shell"
                    };
                });
        }
    }
}
