using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnCardPlayedReduceCounterToRandomAlly : SpirefrostBuilder
    {
        internal static string ID => "On Card Played Reduce Counter To Random Ally";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Card Played Reduce Counter To Allies", ID)
                .WithText("Count down a random ally's <sprite name=counter> by <{a}>")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomAlly;
                });
        }
    }
}
