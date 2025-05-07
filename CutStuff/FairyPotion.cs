using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects.IconEffects;
using UnityEngine;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.CardUpgrades
{
    [ToPoolList(PoolListType.Charms)]
    internal class FairyPotion : SpirefrostBuilder
    {
        internal static string ID => "FairyPotionCharm";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static int Amount => 2;

        internal static object GetBuilder()
        {
            return new CardUpgradeDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/FairyCharm.png")
                .WithTitle("Fairy in a Bottle")
                .WithText($"Start with <{Amount}>{MakeKeywordInsert(FlightKeyword.FullID)}")
                .WithTier(1)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintCanBeHit>()
                    };
                    data.effects = new CardData.StatusEffectStacks[]
                    {
                        SStack(Flight.ID, Amount)
                    };
                });
        }
    }
}
