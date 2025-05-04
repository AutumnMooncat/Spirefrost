using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class PreTriggerCopyTargetAttackEffects : SpirefrostBuilder
    {
        internal static string ID => "Pre Trigger Copy Target Attack Effects";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectCopyAttackEffectsPreTrigger>(ID)
                .WithText("Before attacking, copy the target's <keyword=attack> and attack effects")
                .WithCanBeBoosted(false)
                .WithStackable(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectCopyAttackEffectsPreTrigger>(data =>
                {
                    data.copyAttackValue = true;
                });
        }
    }
}
