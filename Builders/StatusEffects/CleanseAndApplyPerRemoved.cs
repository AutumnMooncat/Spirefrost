using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class InstantCleanseAndApplyAttackPerRemoved : SpirefrostBuilder
    {
        internal static string ID => "Instant Cleanse And Apply Attack Per Removed";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectInstantCleanseAndApplyForEachRemoved>(ID)
                .WithText("<keyword=cleanse> and increase <keyword=attack> by <{a}> for each negative status removed")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantCleanseAndApplyForEachRemoved>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Increase Attack");
                });
        }
    }
}
