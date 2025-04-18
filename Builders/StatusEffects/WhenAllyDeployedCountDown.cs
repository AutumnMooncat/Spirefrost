using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenAllyDeployedCountDown : SpirefrostBuilder
    {
        internal static string ID => "When Ally Deployed Count Down";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Enemy Deployed Ink To Target", ID)
                .WithText("When an ally is deployed, count down <keyword=counter> by <{a}>")
                .WithCanBeBoosted(true)
                .WithStackable(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenDeployed>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("Reduce Counter");
                    data.whenEnemyDeployed = false;
                    data.whenAllyDeployed = true;
                });
        }
    }
}
