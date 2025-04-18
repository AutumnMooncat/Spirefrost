using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenCardDestroyedDraw : SpirefrostBuilder
    {
        internal static string ID => "When Card Destroyed, Draw";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Card Destroyed, Gain Attack", ID)
                .WithText("When a card is destroyed, draw <{a}>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenCardDestroyed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Instant Draw");
                });
        }
    }
}
