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
                .WithText("Before attacking, replace this with the target's attack effects")
                .WithCanBeBoosted(false)
                .WithStackable(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectCopyAttackEffectsPreTrigger>(data =>
                {
                    
                });
        }
    }
}
