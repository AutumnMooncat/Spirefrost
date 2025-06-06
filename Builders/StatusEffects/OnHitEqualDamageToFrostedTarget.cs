using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnHitEqualDamageToFrostedTarget : SpirefrostBuilder
    {
        internal static string ID => "On Hit Equal Damage To Frosted Target";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXOnHitContext>(ID)
                .WithText("Deal additional damage equal to the target's {0}")
                .WithTextInsert("<keyword=frost>")
                .WithStackable(true)
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnHitContext>(data =>
                {
                    ScriptableCurrentStatus scriptable = ScriptableObject.CreateInstance<ScriptableCurrentStatus>();
                    scriptable.statusType = "frost";
                    data.contextEqualAmount = scriptable;
                    data.addDamageFactor = 1;
                    data.applyConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintHasStatus>(c =>
                        {
                            c.status = TryGet<StatusEffectData>("Frost");
                        })
                    };
                });
        }
    }
}
