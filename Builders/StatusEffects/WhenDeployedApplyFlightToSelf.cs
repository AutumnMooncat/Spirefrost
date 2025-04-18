using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects.IconEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenDeployedApplyFlightToSelf : SpirefrostBuilder
    {
        internal static string ID => "When Deployed Apply Flight To Self";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Deployed Apply Block To Self", ID)
                .WithText($"When deployed, gain <{{a}}>{MakeKeywordInsert(FlightKeyword.FullID)}")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenDeployed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(Flight.ID);
                });
        }
    }
}
