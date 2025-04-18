using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnTurnJudgeEnemies : SpirefrostBuilder
    {
        internal static string ID => "On Turn Judge Enemies";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Turn Apply Snow To Enemies", ID)
                .WithText("Set the <keyword=health> of all enemies with <{a}> or less to 0")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnTurn>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(Judgement.ID);
                    data.applyConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintHasHealth>()
                    };
                    data.noTargetType = NoTargetType.None;
                });
        }
    }

    internal class Judgement : SpirefrostBuilder
    {
        internal static string ID => "STS Judgement";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectJudgement>(ID)
                .WithText("If the target has <{a}> or less <keyword=health>, set their <keyword=health> to 0")
                .SubscribeToAfterAllBuildEvent<StatusEffectJudgement>(data =>
                {

                });
        }
    }
}
