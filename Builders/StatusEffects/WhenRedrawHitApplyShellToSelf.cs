using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenRedrawHitApplyShellToSelf : SpirefrostBuilder
    {
        internal static string ID => "When Redraw Hit Apply Shell To Self";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Redraw Hit Apply Attack & Health To Self", ID)
                .WithText("When <Redraw Bell> is hit, gain <{a}><keyword=shell>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenRedrawHit>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Shell");
                });
        }
    }
}
