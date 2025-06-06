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
                .WithText("When a card is destroyed, <keyword=draw {a}>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenCardDestroyed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Instant Draw");
                    data.WithSwappable(TryGet<StatusEffectData>(WhenCardDestroyedAddGunkToHand.ID));
                });
        }
    }

    internal class WhenCardDestroyedAddGunkToHand : SpirefrostBuilder
    {
        internal static string ID => "When Card Destroyed, Add Gunk To Hand";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Card Destroyed, Gain Attack", ID)
                .WithText("When a card is destroyed, add <{a}> <card=Deadweight> to your hand")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenCardDestroyed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Instant Summon Gunk In Hand");
                });
        }
    }
}
