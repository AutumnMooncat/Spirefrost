using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenAllyIsHitApplyShellToThem : SpirefrostBuilder
    {
        internal static string ID => "When Ally Is Hit Apply Shell To Them";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Ally is Hit Apply Vim To Target", ID)
                .WithText("When an ally is hit, apply <{a}><keyword=shell> to them")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenAllyIsHit>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Shell");
                });
        }
    }
}
