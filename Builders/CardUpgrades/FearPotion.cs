using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects;
using Spirefrost.Builders.StatusEffects.IconEffects;
using UnityEngine;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.CardUpgrades
{
    [ToPoolList(PoolListType.Charms)]
    internal class FearPotion : SpirefrostBuilder
    {
        internal static string ID => "FearPotionCharm";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static int Amount => 1;

        internal static object GetBuilder()
        {
            return new CardUpgradeDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/FearCharm.png")
                .WithTitle("Fear Potion")
                .WithText($"On kill, apply <{Amount}><keyword=demonize> to all enemies")
                .WithTier(2)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintDoesDamage>()
                    };
                    data.effects = new CardData.StatusEffectStacks[]
                    {
                        SStack(OnKillApplyDemonizeToEnemies.ID, Amount)
                    };
                });
        }
    }
}
