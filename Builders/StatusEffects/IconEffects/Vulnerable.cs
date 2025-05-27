using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using Spirefrost.StatusEffects;
using UnityEngine;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.StatusEffects.IconEffects
{
    internal class Vulnerable : SpirefrostBuilder
    {
        internal static string ID => "STS Vuln";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectSTSVulnerable>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .WithOffensive(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectSTSVulnerable>(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintCanBeHit>()
                    };
                })
                .Subscribe_WithStatusIcon(VulnerableIcon.ID);
        }
    }
}
