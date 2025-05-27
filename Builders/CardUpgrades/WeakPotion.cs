using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects;
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

        internal static int Amount => 2;

        internal static object GetBuilder()
        {
            return new CardUpgradeDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/WeaknessCharm.png")
                .WithTitle("Weak Potion")
                .WithText($"When an ally is hit, apply <{Amount}><keyword=frost> to the attacker")
                .WithTier(2)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintIsUnit>()
                    };
                    data.effects = new CardData.StatusEffectStacks[]
                    {
                        SStack("When Ally is Hit Apply Frost To Attacker", Amount)
                    };
                });
        }
    }
}
