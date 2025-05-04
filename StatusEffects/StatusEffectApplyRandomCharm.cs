using System;
using System.Collections;
using System.Collections.Generic;

namespace Spirefrost
{
    public class StatusEffectApplyRandomCharm : StatusEffectInstant
    {
        public bool useBanlist = false;

        private static readonly List<string> bannedCharms = new List<string>()
        {
            "CardUpgradeBalanced",
            "CardUpgradeBombskull",
            "CardUpgradeAttackRemoveEffects",
            "CardUpgradeBlue",
            "CardUpgradePig",
            "CardUpgradeBootleg",
            "CardUpgradeMime",
            "CardUpgradeFlameberry",
            "CardUpgradeGlass",
            "CardUpgradeShroomReduceHealth",
            "CardUpgradeFrenzyReduceAttack"
        };

        private bool HasRoom(Entity entity)
        {
            int count = entity.data.upgrades.FindAll((CardUpgradeData a) => a.type == CardUpgradeData.Type.Charm && a.takeSlot).Count;
            int num = entity.data.charmSlots;
            if (entity.data.customData != null)
            {
                num += entity.data.customData.Get("extraCharmSlots", 0);
            }

            if (count >= num)
            {
                return false;
            }

            return true;
        }

        public override IEnumerator Process()
        {
            if (HasRoom(target))
            {
                List<CardUpgradeData> validUpgrades = new List<CardUpgradeData>();
                foreach (CardUpgradeData upgrade in AddressableLoader.GetGroup<CardUpgradeData>("CardUpgradeData"))
                {
                    if (upgrade.type == CardUpgradeData.Type.Charm && upgrade.tier >= 0 && !(useBanlist && bannedCharms.Contains(upgrade.name)) && upgrade.CanAssign(target))
                    {
                        validUpgrades.Add(upgrade);
                    }
                }
                int applyAmount = Math.Min(GetAmount(), validUpgrades.Count);
                for (int i = 0; i < applyAmount; i++)
                {
                    CardUpgradeData applyMe = validUpgrades.TakeRandom().Clone();
                    yield return applyMe.Assign(target);
                }
            }
            yield return base.Process();
        }
    }
}
