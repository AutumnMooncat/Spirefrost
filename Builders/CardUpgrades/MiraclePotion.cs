using Deadpan.Enums.Engine.Components.Modding;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.CardUpgrades
{
    [ToPoolList(PoolListType.Charms)]
    internal class MiraclePotion : SpirefrostBuilder
    {
        internal static string ID => "MiraclePotionCharm";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardUpgradeDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/MiracleCharm.png")
                .WithTitle("Bottled Miracle")
                .WithText($"When an ally is deployed, count down <keyword=counter> by 1")
                .WithTier(2)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintMaxCounterMoreThan>(t => t.moreThan = 0),
                    };
                    data.effects = new CardData.StatusEffectStacks[]
                    {
                        SStack("When Ally Deployed Count Down", 1)
                    };
                });
        }
    }
}
