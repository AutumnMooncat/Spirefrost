using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnCardPlayedIncreaseCounterToRandomEnemy : SpirefrostBuilder
    {
        internal static string ID => "On Card Played Increase Counter To RandomEnemy";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Card Played Boost To RandomEnemy", ID)
                .WithText("Count up <keyword=counter> of a random enemy by <{a}>")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(IncreaseCounter.ID);
                    data.noTargetType = NoTargetType.NoTargetForStatus;
                    data.noTargetTypeArgs = new string[] { "<sprite name=counter>" };
                    data.type = "counter down";
                });
        }
    }
}
