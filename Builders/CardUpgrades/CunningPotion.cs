using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Cards.Items;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.CardUpgrades
{
    [ToPoolList(PoolListType.SilentCharms)]
    internal class CunningPotion : SpirefrostBuilder
    {
        internal static string ID => "CunningPotionCharm";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static int Amount => 3;

        internal static object GetBuilder()
        {
            return new CardUpgradeDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/CunningCharm.png")
                .WithTitle("Cunning Potion")
                .WithText($"When consumed, add <{Amount}> {MakeCardInsert(Shiv.FullID)} to your hand")
                .WithTier(2)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintHasTrait>(t => t.trait = TryGet<TraitData>("Consume")),
                    };
                    data.effects = new CardData.StatusEffectStacks[]
                    {
                        SStack(WhenConsumedAddShivToHand.ID, Amount)
                    };
                });
        }
    }
}
