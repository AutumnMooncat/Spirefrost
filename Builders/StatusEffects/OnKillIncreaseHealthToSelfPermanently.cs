using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnKillIncreaseHealthToSelfPermanently : SpirefrostBuilder
    {
        internal static string ID => "On Kill Increase Health To Self Permanently";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectFeed>(ID)
                .WithText("Permanently increase <keyword=health> by <{a}> on kill")
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectFeed>(data =>
                {

                });
        }
    }
}
