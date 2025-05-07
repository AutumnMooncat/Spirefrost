using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.Traits;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.CardUpgrades
{
    [ToPoolList(PoolListType.DefectCharms)]
    internal class FocusPotion : SpirefrostBuilder
    {
        internal static string ID => "FocusPotionCharm";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static int Amount => 2;

        internal static object GetBuilder()
        {
            return new CardUpgradeDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/FocusCharm.png")
                .WithTitle("Focus Potion")
                .WithText($"Gain <keyword={FocusKeyword.FullID} {Amount}>")
                .WithTier(2)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintIsUnit>(),
                    };
                    data.giveTraits = new CardData.TraitStacks[]
                    {
                        TStack(FocusTrait.ID, Amount)
                    };
                });
        }
    }
}
