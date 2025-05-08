using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using Spirefrost.StatusEffects;
using UnityEngine;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.StatusEffects.IconEffects
{
    internal class Regrow : SpirefrostBuilder
    {
        internal static string ID => "STS Regrow";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectSTSLifeLink>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectSTSLifeLink>(data =>
                {
                    data.preventDeath = true;
                    data.animation = ScriptableObject.CreateInstance<CardAnimationClunkerBossChange>();
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintHasHealth>()
                    };
                })
                .Subscribe_WithStatusIcon(RegrowIcon.ID);
        }
    }
}
