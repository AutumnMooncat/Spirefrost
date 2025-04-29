using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhileInHandIncreaseItemDamage : SpirefrostBuilder
    {
        internal static string ID => "While In Hand Increase Item Damage";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("While In Hand Reduce Attack To Allies", ID)
                .WithText("While in hand, add <+{a}><keyword=attack> to <Items> in your hand")
                .SubscribeToAfterAllBuildEvent<StatusEffectWhileInHandX>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Hand;
                    data.effectToApply = TryGet<StatusEffectData>("Ongoing Increase Attack");
                    data.applyConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintIsItem>()
                    };
                });
        }
    }
}
