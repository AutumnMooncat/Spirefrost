using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using Spirefrost.StatusEffects;
using UnityEngine;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.StatusEffects.IconEffects
{
    internal class Energized : SpirefrostBuilder
    {
        internal static string ID => "STS Energized";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectSTSEnergized>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectSTSEnergized>(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintMaxCounterMoreThan>()
                    };
                })
                .Subscribe_WithStatusIcon(EnergizedIcon.ID);
        }
    }
}
