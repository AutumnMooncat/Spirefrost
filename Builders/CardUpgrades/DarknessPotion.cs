using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.CardUpgrades
{
    [ToPoolList(PoolListType.DefectCharms)]
    internal class DarknessPotion : SpirefrostBuilder
    {
        internal static string ID => "DarknessPotionCharm";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static int Amount => 1;

        internal static object GetBuilder()
        {
            return new CardUpgradeDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/DarknessCharm.png")
                .WithTitle("Essence of Darkness")
                .WithText($"When deployed, {MakeKeywordInsert(ChannelKeyword.FullID)} <{Amount}>{MakeKeywordInsert(DarkKeyword.FullID)}")
                .WithTier(2)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintOr>(or =>
                        {
                            or.constraints = new TargetConstraint[]
                            {
                                MakeConstraint<TargetConstraintMaxCounterMoreThan>(c => c.moreThan = 0),
                                MakeConstraint<TargetConstraintHasReaction>()
                            };
                        })
                    };
                    data.effects = new CardData.StatusEffectStacks[]
                    {
                        SStack(WhenDeployedChannelDark.ID, Amount)
                    };
                });
        }
    }
}
