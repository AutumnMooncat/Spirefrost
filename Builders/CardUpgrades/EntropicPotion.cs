using Deadpan.Enums.Engine.Components.Modding;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.CardUpgrades
{
    [ToPoolList(PoolListType.Charms)]
    internal class EntropicPotion : SpirefrostBuilder
    {
        internal static string ID => "EntropicBrewCharm";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static int Amount => 3;

        private static readonly List<string> bannedCharms = new List<string>()
        {
            "CardUpgradeCrush", // Bricks items
            "CardUpgradeShredder", // Can brick
            "CardUpgradeMime", // Can brick units with trigger but no counter
            "CardUpgradeAttackRemoveEffects", // Can brick units with trigger but no counter
            "CardUpgradeMuncher", // Lol
            EntropicPotion.FullID, // Dont loop
        };

        internal static object GetBuilder()
        {
            return new CardUpgradeDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/EntropicCharm.png")
                .WithTitle("Entropic Brew")
                .WithText($"Apply <{Amount}> other random <Charms> to this card\nThey do not take up charm slots")
                .WithTier(2)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    CardScriptRunnable entropicScript = ScriptableObject.CreateInstance<CardScriptRunnable>();
                    entropicScript.runnable = card =>
                    {
                        List<CardUpgradeData> validUpgrades = new List<CardUpgradeData>();
                        foreach (CardUpgradeData upgrade in AddressableLoader.GetGroup<CardUpgradeData>("CardUpgradeData"))
                        {
                            if (upgrade.type == CardUpgradeData.Type.Charm && upgrade.tier >= 0 && !bannedCharms.Contains(upgrade.name) && upgrade.CanAssign(card))
                            {
                                validUpgrades.Add(upgrade);
                            }
                        }
                        int applyAmount = Math.Min(Amount, validUpgrades.Count);
                        for (int i = 0; i < applyAmount; i++)
                        {
                            CardUpgradeData applyMe = validUpgrades.TakeRandom().Clone();
                            applyMe.takeSlot = false;
                            applyMe.Assign(card);
                        }
                    };
                    data.scripts = new CardScript[]
                    {
                        entropicScript
                    };
                });
        }
    }
}
