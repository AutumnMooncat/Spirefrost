using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenHitReduceEffectToAttacker : SpirefrostBuilder
    {
        internal static string ID => "When Hit Reduce Effect To Attacker";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Hit Reduce Attack To Attacker", ID)
                .WithText("When hit, reduce the attacker's effects by <{a}>")
                .WithCanBeBoosted(true)
                .WithStackable(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenHit>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Reduce Effects");
                });
        }
    }
}
