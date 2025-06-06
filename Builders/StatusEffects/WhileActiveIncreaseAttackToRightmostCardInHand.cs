using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhileActiveIncreaseAttackToRightmostCardInHand : SpirefrostBuilder
    {
        internal static string ID => "While Active Increase Attack To Rightmost Card In Hand";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectWhileActiveXCustom>(ID)
                .WithText("While active, add <+{a}><keyword=attack> to the rightmost card in your hand")
                .WithCanBeBoosted(true)
                .WithStackable(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectWhileActiveXCustom>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Hand;
                    data.applyConstraints = new TargetConstraint[] 
                    {
                        MakeConstraint<TargetConstraintLastInHand>()
                    };
                    data.effectToApply = TryGet<StatusEffectData>("Ongoing Increase Attack");
                    data.WithSwappable(TryGet<StatusEffectData>("While Active Increase Attack To AlliesInRow"));
                });
        }
    }
}
