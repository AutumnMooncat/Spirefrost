using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnHitEqualHealToSelf : SpirefrostBuilder
    {
        internal static string ID => "On Hit Equal Heal To Self";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Hit Equal Heal To FrontAlly", ID)
                .WithText("Restore <keyword=health> equal to damage dealt")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnHit>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                });
        }
    }
}
