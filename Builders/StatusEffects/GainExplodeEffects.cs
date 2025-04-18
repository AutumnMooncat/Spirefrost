using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class GainExplode : SpirefrostBuilder
    {
        internal static string ID => "Gain Explode";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectInstantGainTrait>(ID)
                .WithText("Apply <{a}> <keyword=explode>")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantGainTrait>(data =>
                {
                    data.traitToGain = TryGet<TraitData>("Explode");
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintIsUnit>(),
                        ScriptableObject.CreateInstance<TargetConstraintCanBeHit>()
                    };
                });
        }
    }

    internal class OnTurnApplyExplodeToSelf : SpirefrostBuilder
    {
        internal static string ID => "On Turn Apply Explode To Self";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Turn Apply Teeth To Self", ID)
                .WithText("Gain <{a}> <keyword=explode>")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnTurn>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(GainExplode.ID);
                });
        }
    }
}
