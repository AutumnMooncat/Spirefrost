using Deadpan.Enums.Engine.Components.Modding;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.CardUpgrades
{
    [ToPoolList(PoolListType.WatcherCharms)]
    internal class StancePotion : SpirefrostBuilder
    {
        internal static string ID => "StancePotionCharm";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static int Debuff => 1;

        internal static int Effect => 3;

        internal static object GetBuilder()
        {
            return new CardUpgradeDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/StanceCharm.png")
                .WithTitle("Stance Potion")
                .WithText($"<+{Effect}><keyword=attack>\nStart with <{Debuff}><keyword=demonize>")
                .WithTier(2)
                .ChangeDamage(Effect)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintDoesDamage>(),
                        MakeConstraint<TargetConstraintCanBeHit>(),
                        MakeConstraint<TargetConstraintHasHealth>()
                    };
                    data.effects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Demonize", Debuff)
                    };
                });
        }
    }
}
