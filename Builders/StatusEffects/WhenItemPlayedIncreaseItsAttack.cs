using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenItemPlayedIncreaseItsAttack : SpirefrostBuilder
    {
        internal static string ID => "When Item Played, Increase Its Attack";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXWhenAnyCardIsPlayed>(ID)
                .WithText("When an item is played, add <+{a}><keyword=attack> to it")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenAnyCardIsPlayed>(data =>
                {
                    data.targetPlayedCard = true;
                    data.effectToApply = TryGet<StatusEffectData>("Increase Attack");
                    data.applyConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintIsItem>(),
                        ScriptableObject.CreateInstance<TargetConstraintDoesDamage>()
                    };
                });
        }
    }
}
