using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnHitEqualShellToFrontAlly : SpirefrostBuilder
    {
        internal static string ID => "On Hit Equal Shell To FrontAlly";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Hit Equal Heal To FrontAlly", ID)
                .WithText("Apply <keyword=shell> to front ally equal to damage dealt")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnHit>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Shell");
                    data.noTargetType = NoTargetType.NoTargetForStatus;
                    data.noTargetTypeArgs = new string[] { "<sprite name=shell>" };
                });
        }
    }
}
