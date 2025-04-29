using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenCardDestroyedGainShell : SpirefrostBuilder
    {
        internal static string ID => "When Card Destroyed, Gain Shell";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Card Destroyed, Gain Frenzy", ID)
                .WithText("When a card is destroyed, gain <{a}><keyword=shell>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenCardDestroyed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Shell");
                });
        }
    }
}
