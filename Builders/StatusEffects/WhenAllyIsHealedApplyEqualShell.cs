using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenAllyIsHealedApplyEqualShell : SpirefrostBuilder
    {
        internal static string ID => "When Ally Is Healed Apply Equal Shell";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Ally Is Healed Apply Equal Spice", ID)
                .WithText("When an ally is healed, apply equal <keyword=shell>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenAllyHealed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Shell");
                });
        }
    }
}
