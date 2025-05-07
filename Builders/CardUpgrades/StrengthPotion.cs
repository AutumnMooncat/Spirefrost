using Deadpan.Enums.Engine.Components.Modding;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.CardUpgrades
{
    [ToPoolList(PoolListType.Charms)]
    internal class StrengthPotion : SpirefrostBuilder
    {
        internal static string ID => "StrengthPotionCharm";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static int Amount => 2;

        internal static object GetBuilder()
        {
            return new CardUpgradeDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/StrengthCharm.png")
                .WithTitle("Strength Potion")
                .WithText($"Gain <+{Amount}><keyword=attack> on kill")
                .WithTier(2)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintDoesDamage>()
                    };
                    data.effects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Kill Apply Attack To Self", Amount)
                    };
                });
        }
    }
}
