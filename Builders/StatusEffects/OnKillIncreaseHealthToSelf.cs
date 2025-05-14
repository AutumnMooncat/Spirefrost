using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnKillIncreaseHealthToSelf : SpirefrostBuilder
    {
        internal static string ID => "On Kill Increase Health To Self";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Kill Increase Health To Self & Allies", ID)
                .WithText("Increase <keyword=health> by <{a}> on kill")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnKill>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                });
        }
    }
}
