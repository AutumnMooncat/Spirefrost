using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using UnityEngine;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.StatusEffects.IconEffects
{
    internal class Ritual : SpirefrostBuilder
    {
        internal static string ID => "STS Ritual";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXWhenRedrawHitButIgnoreInk>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .WithIsKeyword(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenRedrawHitButIgnoreInk>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("Increase Attack");
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintDoesDamage>()
                    };
                })
                .Subscribe_WithStatusIcon(RitualIcon.ID);
        }
    }
}
