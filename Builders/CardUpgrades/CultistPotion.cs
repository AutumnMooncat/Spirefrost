using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects.IconEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.CardUpgrades
{
    [ToPoolList(PoolListType.Charms)]
    internal class CultistPotion : SpirefrostBuilder
    {
        internal static string ID => "CultistPotionCharm";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static int Amount => 2;

        internal static int DebuffAmount => 1;

        internal static object GetBuilder()
        {
            return new CardUpgradeDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/CultistCharm.png")
                .WithTitle("Cultist Potion")
                .WithText($"Increase <keyword=counter> by <{DebuffAmount}>\nStart with <{Amount}>{MakeKeywordInsert(RitualKeyword.FullID)}")
                .WithTier(2)
                .ChangeCounter(DebuffAmount)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintDoesDamage>(),
                        MakeConstraint<TargetConstraintIsUnit>()
                    };
                    data.effects = new CardData.StatusEffectStacks[]
                    {
                        SStack(Ritual.ID, Amount)
                    };
                });
        }
    }
}
