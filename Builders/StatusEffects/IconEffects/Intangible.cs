using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using Spirefrost.Patches;
using Spirefrost.StatusEffects;
using UnityEngine;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.StatusEffects.IconEffects
{
    internal class Intangible : SpirefrostBuilder
    {
        internal static string ID => "STS Intangible";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectSTSIntangible>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectSTSIntangible>(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintIsUnit>(t => {
                            t.mustBeMiniboss = true;
                            t.not = true;
                        })
                    };
                    data.WithSwappable(TryGet<StatusEffectData>("Block"), null, new Vector2Int(3, 3));
                })
                .Subscribe_WithStatusIcon(IntangibleIcon.ID);
        }
    }
}
