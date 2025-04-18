using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects.IconEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.CardUpgrades
{
    [ToPoolList(PoolListType.Charms)]
    internal class WeakPotion : SpirefrostBuilder
    {
        internal static string ID => "WeakPotionCharm";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardUpgradeDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/WeaknessCharm.png")
                .WithTitle("Weak Potion")
                .WithText($"Apply <1>{MakeKeywordInsert(WeakKeyword.FullID)}")
                .WithTier(2)
                .SetBecomesTarget(true)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintPlayOnSlot>(t => t.board = true),
                        MakeConstraint<TargetConstraintPlayOnSlot>(t => { t.slot = true; t.not = true; }),
                        MakeConstraint<TargetConstraintOr>(or =>
                        {
                            or.constraints = new TargetConstraint[]
                            {
                                MakeConstraint<TargetConstraintMaxCounterMoreThan>(t => t.moreThan = 0),
                                MakeConstraint<TargetConstraintHasReaction>(),
                                MakeConstraint<TargetConstraintIsItem>()
                            };
                        })
                    };
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(Weak.ID, 1)
                    };
                });
        }
    }
}
