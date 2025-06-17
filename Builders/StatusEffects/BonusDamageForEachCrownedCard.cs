using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class BonusDamageForEachCrownedCard : SpirefrostBuilder
    {
        internal static string ID => "Bonus Damage For Each Crowned Card";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectLessJankyBonusDamageEqualToX>(ID)
                .WithText("Deals <{a}> additional damage for each <sprite name=crown>'d card")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectLessJankyBonusDamageEqualToX>(data =>
                {
                    data.on = StatusEffectLessJankyBonusDamageEqualToX.On.ScriptableAmount;
                    ScriptableOwnedCards owned = ScriptableObject.CreateInstance<ScriptableOwnedCards>();
                    owned.constraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintHasCrown>()
                    };
                    data.scriptableAmount = owned;
                    data.scaleByCount = true;
                });
        }
    }
}
