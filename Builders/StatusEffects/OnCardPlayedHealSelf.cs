using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnCardPlayedHealSelf : SpirefrostBuilder
    {
        internal static string ID => "On Card Played Heal Self";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Card Played Heal & Cleanse To Allies", ID)
                .WithText($"Restore <{{a}}><keyword=health> to self")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("Heal (No Ping)");
                });
        }
    }
}
