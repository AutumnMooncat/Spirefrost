using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnCardPlayedGainRandomCharm : SpirefrostBuilder
    {
        internal static string ID => "On Card Played Gain Random Charm";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Card Played Apply Attack To Self", ID)
                .WithText("Gain a random <Charm> for the rest of combat")
                .WithStackable(false)
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(InstantApplyRandomCharm.ID);
                });
        }
    }

    internal class InstantApplyRandomCharm : SpirefrostBuilder
    {
        internal static string ID => "Instant Apply Random Charm";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyRandomCharm>(ID)
                .WithText("Apply a random <Charm>")
                .WithStackable(false)
                .WithCanBeBoosted(false);
        }
    }
}
