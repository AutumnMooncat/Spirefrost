using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnCardPlayedLoseHealthSelf : SpirefrostBuilder
    {
        internal static string ID => "On Card Played Lose Health Self";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Card Played Lose Health", ID)
                .WithText("Lose {a}<keyword=health>")
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>(InstantLoseHealth.ID);
                    data.doPing = true;
                });
        }
    }

    internal class InstantLoseHealth : SpirefrostBuilder
    {
        internal static string ID => "Instant Lose Health";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectInstantLoseHealth>(ID)
                .WithCanBeBoosted(true)
                .WithStackable(true)
                .WithText("Reduce current <keyword=health> by <{a}>");
        }
    }
}
