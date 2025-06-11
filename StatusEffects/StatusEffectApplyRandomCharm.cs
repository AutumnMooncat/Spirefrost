using Spirefrost.Builders.CardUpgrades;
using Spirefrost.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Spirefrost.StatusEffects
{
    public class StatusEffectApplyRandomCharm : StatusEffectInstant
    {
        public bool useBanlist = true;

        private static readonly List<string> bannedCharms = new List<string>()
        {
            "CardUpgradeBalanced", // Lowers HP
            "CardUpgradeBombskull", // Useless
            "CardUpgradeAttackRemoveEffects", // Troll
            "CardUpgradeBlue", // Lowers HP
            "CardUpgradePig", // Useless
            "CardUpgradeBootleg", // Lowers HP
            "CardUpgradeMime", // Troll
            "CardUpgradeFlameberry", // Lowers HP
            "CardUpgradeGlass", // Lowers HP
            "CardUpgradeShroomReduceHealth", // Lowers HP
            "CardUpgradeFrenzyReduceAttack", // Lowers HP
            "CardUpgradeHook", // Useless
            StancePotion.FullID // Applies Demonize
        };

        private RestoreData restore;

        private class RestoreData
        {
            public int deltaMaxDamage;
            public int deltaDamage;
            public int deltaMaxHP;
            public int deltaHP;
            public int deltaMaxCounter;
            public int deltaCounter;
            public int deltaMaxUses;
            public int deltaUses;
            public int deltaEffectBonus;
            public float deltaEffectFactor;
            public List<CardData.StatusEffectStacks> deltaAttackEffects;
            public List<StatusEffectData> deltaEffects;
            public List<Entity.TraitStacks> deltaTraits;
        }

        private bool HasRoom(CardData data)
        {
            int filled = data.upgrades.FindAll((a) => a.type == CardUpgradeData.Type.Charm && a.takeSlot).Count;
            int slots = data.charmSlots;
            if (data.customData != null)
            {
                data.TryGetCustomData("extraCharmSlots", out int found, 0);
                slots += found;
            }

            if (filled >= slots)
            {
                return false;
            }

            count = Math.Min(count, slots - filled);
            return true;
        }

        public override IEnumerator Process()
        {
            Card card = target.display as Card;
            StatusEffectCustomSaveable saveable = StatusEffectCustomSaveable.GetOrMake(target);
            CardData mirrorData = target.GetOrMakeMirroredData();
            CardData originalData = target.data;
            if (HasRoom(mirrorData) && card)
            {
                List<CardUpgradeData> validUpgrades = new List<CardUpgradeData>();
                foreach (CardUpgradeData upgrade in AddressableLoader.GetGroup<CardUpgradeData>("CardUpgradeData"))
                {
                    if (upgrade.type == CardUpgradeData.Type.Charm && upgrade.tier >= 0 && !(useBanlist && bannedCharms.Contains(upgrade.name)) && upgrade.CanAssign(mirrorData) && !ClassBasedBanCheck(upgrade))
                    {
                        validUpgrades.Add(upgrade);
                    }
                }
                int applyAmount = Math.Min(count, validUpgrades.Count);
                for (int i = 0; i < applyAmount; i++)
                {
                    CardUpgradeData applyMe = validUpgrades.TakeRandom().Clone();
                    saveable.MakeSaveable(typeof(TemporaryCharmPatches), nameof(TemporaryCharmPatches.OnLoad), applyMe.name);
                    target.data = mirrorData;
                    StoreChanges();
                    Events.InvokeUpgradeAssign(target, applyMe);
                    applyMe.Assign(target.data);
                    // Manually call clear to prevent onRemove from firing, as we will add back without firing onApply
                    target.statusEffects.Clear();
                    target.startingEffectsApplied = false;
                    yield return card.UpdateData();
                    RestoreChanges();
                    yield return target.UpdateTraits();
                    CardUpdateDataPatch.shortCircuitOnce = true;
                    yield return card.UpdateData();
                    target.data = originalData;
                }
            }
            yield return base.Process();
        }

        private bool ClassBasedBanCheck(CardUpgradeData upgrade)
        {
            if (upgrade.effects.Any(stack => stack.data is StatusEffectApplyXWhenDeployed || stack.data is StatusEffectTriggerWhenDeployed))
            {
                return true;
            }
            if (upgrade.giveTraits.Any(stack => stack.data.effects.Any(data => data is StatusEffectApplyXWhenDeployed || data is StatusEffectTriggerWhenDeployed)))
            {
                return true;
            }
            return false;
        }

        private void StoreChanges()
        {
            restore = new RestoreData()
            {
                deltaMaxDamage = target.damage.max - target.data.damage,
                deltaDamage = target.damage.current - target.damage.max,
                deltaMaxHP = target.hp.max - target.data.hp,
                deltaHP = target.hp.current - target.data.hp,
                deltaMaxCounter = target.counter.max - target.data.counter,
                deltaCounter = target.counter.current - target.counter.max,
                deltaMaxUses = target.uses.max - target.data.uses,
                deltaUses = target.uses.current - target.uses.max,
                deltaEffectBonus = target.effectBonus - target.data.effectBonus,
                deltaEffectFactor = target.effectFactor - target.data.effectFactor,
                deltaAttackEffects = new List<CardData.StatusEffectStacks>(),
                deltaEffects = new List<StatusEffectData>(),
                deltaTraits = new List<Entity.TraitStacks>()
            };

            restore.deltaAttackEffects.AddRange(target.attackEffects);
            foreach (var item in target.data.attackEffects)
            {
                CardData.StatusEffectStacks found = restore.deltaAttackEffects.Where(effect => effect.data.name == item.data.name).FirstOrDefault();
                if (found != null)
                {
                    found.count -= item.count;
                    if (found.count == 0)
                    {
                        restore.deltaAttackEffects.Remove(found);
                    }
                } 
                else
                {
                    CardData.StatusEffectStacks copy = item.Clone();
                    item.count *= -1;
                    restore.deltaAttackEffects.Add(copy);
                }
            }

            restore.deltaEffects.AddRange(target.statusEffects);
            foreach (var item in target.data.startWithEffects)
            {
                StatusEffectData found = restore.deltaEffects.Where(effect => effect.name == item.data.name).FirstOrDefault();
                if (found != null)
                {
                    found.count -= item.count;
                    if (found.count == 0 && found.temporary == 0)
                    {
                        restore.deltaEffects.Remove(found);
                    }
                }
                else
                {
                    CardData.StatusEffectStacks copy = item.Clone();
                    copy.data.count = -item.count;
                    restore.deltaEffects.Add(copy.data);
                }
            }

            foreach (var traitStack in target.traits)
            {
                foreach (var effect in traitStack.passiveEffects)
                {
                    restore.deltaEffects.Remove(effect);
                    StatusEffectSystem.activeEffects.Remove(effect);
                    Destroy(effect);
                }
            }

            restore.deltaTraits.AddRange(target.traits);
            foreach (var item in target.data.traits)
            {
                Entity.TraitStacks found = restore.deltaTraits.Where(trait => trait.data.name == item.data.name).FirstOrDefault();
                if (found != null)
                {
                    found.count -= item.count;
                    if (found.count == 0)
                    {
                        restore.deltaTraits.Remove(found);
                    }
                }
                else
                {
                    Entity.TraitStacks copyish = new Entity.TraitStacks(item.data, -item.count);
                    restore.deltaTraits.Add(copyish);
                }
            }
        }

        private void RestoreChanges()
        {
            target.hp.max += restore.deltaMaxHP;
            bool hadHP = target.hp.current > 0;
            target.hp.current += restore.deltaHP;
            if (target.hp.current <= 0 && hadHP)
            {
                target.hp.current = 1;
            }
            target.damage.max += restore.deltaMaxDamage;
            target.damage.current += restore.deltaDamage;
            bool hadCounter = target.counter.max > 0;
            target.counter.max += restore.deltaMaxCounter;
            if (target.counter.max <= 0 && hadCounter)
            {
                target.counter.max = 1;
            }
            target.counter.current += restore.deltaCounter;
            target.uses.max += restore.deltaMaxUses;
            target.uses.current += restore.deltaUses;
            target.effectBonus += restore.deltaEffectBonus;
            target.effectFactor += restore.deltaEffectFactor;

            foreach (var item in restore.deltaAttackEffects)
            {
                CardData.StatusEffectStacks found = target.attackEffects.Where(effect => effect.data.name == item.data.name).FirstOrDefault();
                if (found != null)
                {
                    found.count += item.count;
                    if (found.count <= 0)
                    {
                        target.attackEffects.Remove(found);
                    }
                }
                else
                {
                    target.attackEffects.Add(item);
                }
            }

            List<StatusEffectData> foundUnstackables = new List<StatusEffectData>();
            foreach (var item in restore.deltaEffects)
            {
                StatusEffectData found = target.statusEffects.Where(effect => effect.name == item.name && !foundUnstackables.Contains(effect)).FirstOrDefault();
                if (found != null)
                {
                    if (found is INonStackingStatusEffect)
                    {
                        foundUnstackables.Add(found);
                    }
                    found.count += item.count;
                    found.temporary += item.temporary;
                    if (found.count <= 0 && found.temporary <= 0)
                    {
                        target.statusEffects.Remove(found);
                        StatusEffectSystem.activeEffects.Remove(found);
                        Destroy(found);
                    }
                }
                else
                {
                    target.statusEffects.Add(item);
                }
            }

            foreach (var item in restore.deltaTraits)
            {
                Entity.TraitStacks found = target.traits.Where(trait => trait.data.name == item.data.name).FirstOrDefault();
                if (found != null)
                {
                    found.count += item.count;
                    if (found.count <= 0)
                    {
                        target.traits.Remove(found);
                    }
                }
                else
                {
                    target.traits.Add(item);
                }
            }

            Card card = target.display as Card;
            card.currentEffectBonus += restore.deltaEffectBonus;
            card.currentEffectFactor += restore.deltaEffectFactor;
            card.currentSilenced = target.silenced;

            restore = null;
        }
    }
}
