using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenDrawnDraw : SpirefrostBuilder
    {
        internal static string ID => "When Drawn, Draw";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Drawn Apply Snow To Allies", ID)
                .WithText("When drawn, <keyword=draw {a}>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenDrawn>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("Instant Draw");
                });
        }
    }
}
