using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenHealthLostGainAttack : SpirefrostBuilder
    {
        internal static string ID => "When Health Lost Gain Attack";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Health Lost Apply Equal Attack To Self And Allies", ID)
                .WithText("When <keyword=health> lost, gain <+{a}><keyword=attack>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenHealthLost>(data =>
                {
                    data.applyEqualAmount = false;
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                });
        }
    }
}
