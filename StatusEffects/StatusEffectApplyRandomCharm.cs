using Spirefrost.Builders.CardUpgrades;
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
            StancePotion.FullID, // Applies Demonize
            EntropicPotion.FullID, // Can pull banned charms, also buggy
            DuplicationPotion.FullID, // This is probably invalid anyway but to be safe
        };

        private class CardDataBackup
        {
            public CardDataBackup(CardData data)
            {
                save = data.Save();
                effectbonus = data.effectBonus;
                effectFactor = data.effectFactor;
            }

            public CardSaveData save;
            public int effectbonus;
            public float effectFactor;
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
            if (HasRoom(target.data) && card)
            {
                target.data = target.GetOrMakeMirroredData();

                List<CardUpgradeData> validUpgrades = new List<CardUpgradeData>();
                foreach (CardUpgradeData upgrade in AddressableLoader.GetGroup<CardUpgradeData>("CardUpgradeData"))
                {
                    if (upgrade.type == CardUpgradeData.Type.Charm && upgrade.tier >= 0 && !(useBanlist && bannedCharms.Contains(upgrade.name)) && upgrade.CanAssign(target) && !ClassBasedBanCheck(upgrade))
                    {
                        validUpgrades.Add(upgrade);
                    }
                }
                int applyAmount = Math.Min(count, validUpgrades.Count);
                for (int i = 0; i < applyAmount; i++)
                {
                    CardDataBackup dataBackup = new CardDataBackup(target.data);
                    CardUpgradeData applyMe = validUpgrades.TakeRandom().Clone();
                    Events.InvokeUpgradeAssign(target, applyMe);
                    applyMe.Assign(target.data);
                    applyMe.Display(target);
                    yield return ReflectChanges(dataBackup);
                    card.promptUpdateDescription = true;
                    target.PromptUpdate();
                }
            }
            yield return base.Process();
        }

        private bool ClassBasedBanCheck(CardUpgradeData upgrade)
        {
            if (upgrade.effects.Any(stack => ClassBasedBanCheck(stack.data)))
            {
                return true;
            }
            if (upgrade.giveTraits.Any(stack => stack.data.effects.Any(data => ClassBasedBanCheck(data))))
            {
                return true;
            }
            return false;
        }

        private bool ClassBasedBanCheck(StatusEffectData data)
        {
            if (data is StatusEffectApplyXWhenDeployed)
            {
                return true;
            }
            if (data is StatusEffectTriggerWhenDeployed)
            {
                return true;
            }
            return false;
        }

        private IEnumerator ReflectChanges(CardDataBackup backup)
        {
            CardSaveData original = backup.save;

            // Check for changes to hp
            int deltaHP = target.data.hp - original.hp;
            bool hadHP = original.hp > 0;
            target.hp.max += deltaHP;
            target.hp.current += deltaHP;
            if (target.hp.max <= 0 && hadHP)
            {
                target.hp.max = 1;
            }
            if (target.hp.current <= 0 && hadHP)
            {
                target.hp.current = 1;
            }

            // Check for changes to damage
            int deltaDamage = target.data.damage - original.damage;
            target.damage.max += deltaDamage;
            target.damage.current += deltaDamage;
            if (target.damage.max < 0)
            {
                target.damage.max = 0;
            }

            // Check for changes to counter
            int deltaCounter = target.data.counter - original.counter;
            bool hadCounter = original.counter > 0;
            target.counter.max += deltaCounter;
            if (target.counter.current > 0)
            {
                // Dont modify if we just triggered
                target.counter.current += deltaCounter;
            }
            if (target.counter.max <= 0 && hadCounter)
            {
                target.counter.max = 1;
            }
            if (target.counter.current < 0)
            {
                target.counter.current = 0;
            }
            foreach (var item in target.statusEffects)
            {
                if (item is StatusEffectExtraCounter extra)
                {
                    extra.ModifyMaxCounter(deltaCounter);
                }
            }

            // Check for changes to effect affectors
            int deltaEffectBonus = target.data.effectBonus - backup.effectbonus;
            target.effectBonus += deltaEffectBonus;
            float deltaEffectFactor = target.data.effectFactor - backup.effectFactor;
            target.effectFactor += deltaEffectFactor;

            // Check for missing or modified attack effects
            foreach (var item in original.attackEffects)
            {
                var found = target.data.attackEffects.Where(effect => effect.data.name == item.name).FirstOrDefault();
                var active = target.attackEffects.Where(effect => effect.data.name == item.name).FirstOrDefault();
                if (active != null)
                {
                    if (found == null)
                    {
                        // Remove from entity
                        target.attackEffects.Remove(active);
                    }
                    else
                    {
                        // Check for number changes
                        int delta = found.count - item.count;
                        active.count += delta;
                    }
                }
            }

            // Check for new attack effects
            foreach (var item in target.data.attackEffects)
            {
                var found = original.attackEffects.Where(effect => effect.name == item.data.name).FirstOrDefault();
                if (found == null)
                {
                    target.attackEffects.Add(item.Clone());
                }
            }

            // Check for missing or modified starter effects
            foreach (var item in original.startWithEffects)
            {
                var found = target.data.startWithEffects.Where(effect => effect.data.name == item.name).FirstOrDefault();
                var active = target.statusEffects.Where(effect => effect.name == item.name).FirstOrDefault();
                if (active != null)
                {
                    if (found == null)
                    {
                        // Remove from entity
                        yield return active.Remove();
                    }
                    else
                    {
                        // Check for number changes
                        int delta = found.count - item.count;
                        if (delta > 0)
                        {
                            yield return StatusEffectSystem.Apply(target, target, active, delta);
                        }
                        else if (delta < 0)
                        {
                            yield return active.RemoveStacks(-delta, false);
                        }
                    }
                }
            }

            // Check for new starter effects
            foreach (var item in target.data.startWithEffects)
            {
                var found = original.startWithEffects.Where(effect => effect.name == item.data.name).FirstOrDefault();
                if (found == null)
                {
                    yield return StatusEffectSystem.Apply(target, null, item.data, item.count, temporary: false, null, fireEvents: true, applyEvenIfZero: true);
                }
            }

            // Check for missing or modified traits
            foreach (var item in original.traits)
            {
                var found = target.data.traits.Where(effect => effect.data.name == item.name).FirstOrDefault();
                var active = target.traits.Where(effect => effect.data.name == item.name).FirstOrDefault();
                if (active != null)
                {
                    if (found == null)
                    {
                        // Remove from entity
                        active.count = 0;
                    }
                    else
                    {
                        // Check for number changes
                        int delta = found.count - item.count;
                        active.count += delta;
                    }
                }
            }

            // Check for new traits
            foreach (var item in target.data.traits)
            {
                var found = original.traits.Where(trait => trait.name == item.data.name).FirstOrDefault();
                if (found == null)
                {
                    target.traits.Add(new Entity.TraitStacks(item.data, item.count));
                }
            }

            // Perform required updates
            yield return target.UpdateTraits();
        }
    }
}
