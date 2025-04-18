﻿using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.CardUpgrades
{
    [ToPoolList(PoolListType.Charms)]
    internal class IronPotion : SpirefrostBuilder
    {
        internal static string ID => "IronPotionCharm";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardUpgradeDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/HeartOfIronCharm.png")
                .WithTitle("Heart of Iron")
                .WithText($"When <Redraw Bell> is hit, gain <2><keyword=shell>")
                .WithTier(1)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintCanBeHit>()
                    };
                    data.effects = new CardData.StatusEffectStacks[]
                    {
                        SStack(WhenRedrawHitApplyShellToSelf.ID, 2)
                    };
                });
        }
    }
}
