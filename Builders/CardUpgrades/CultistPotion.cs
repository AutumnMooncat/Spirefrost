using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects.IconEffects;
using UnityEngine;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;
using static UnityEngine.UI.CanvasScaler;

namespace Spirefrost.Builders.CardUpgrades
{
    [ToPoolList(PoolListType.Charms)]
    internal class CultistPotion : SpirefrostBuilder
    {
        internal static string ID => "CultistPotionCharm";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardUpgradeDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/CultistCharm.png")
                .WithTitle("Cultist Potion")
                .WithText($"Start with <1>{MakeKeywordInsert(RitualKeyword.FullID)}\nReduce <keyword=attack> by <1>")
                .WithTier(2)
                .ChangeDamage(-1)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintAttackMoreThan>(t => t.value = 0),
                        MakeConstraint<TargetConstraintIsUnit>()
                    };
                    data.effects = new CardData.StatusEffectStacks[]
                    {
                        SStack(Ritual.ID, 1)
                    };
                });
        }
    }
}
