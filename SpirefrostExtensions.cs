using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SlayTheFrost
{
    internal static class CardDataExtensions
    {
        internal static void GiveUpgrade(this CardData target, string name = "Crown") // Give a crown by default
        {
            MainModFile.instance.TryGet<CardUpgradeData>(name).Clone().Assign(target);
        }

        internal static void SetRandomHealth(this CardData target, int min, int max)
        {
            if (target.hasHealth)
            {
                target.hp = new Vector2Int(min, max).Random();
                target.hp = Mathf.Max(1, target.hp);
            }
        }

        internal static void SetRandomDamage(this CardData target, int min, int max)
        {
            if (target.hasAttack)
            {
                target.damage = new Vector2Int(min, max).Random();
                target.damage = Mathf.Max(0, target.damage);
            }
        }

        internal static void SetRandomCounter(this CardData target, int min, int max)
        {
            if (target.counter >= 1)
            {
                target.counter = new Vector2Int(min, max).Random();
                target.counter = Mathf.Max(1, target.counter);
            }
        }

        internal static void SetRandomPassive(this CardData target, string passiveEffect, int min, int max)
        {
            StatusEffectData effect = MainModFile.instance.TryGet<StatusEffectData>(passiveEffect);
            target.startWithEffects = target.startWithEffects.With(new CardData.StatusEffectStacks(effect, new Vector2Int(min, max).Random()));
        }

        internal static void SetRandomActive(this CardData target, string passiveEffect, int min, int max)
        {
            StatusEffectData effect = MainModFile.instance.TryGet<StatusEffectData>(passiveEffect);
            target.attackEffects = target.attackEffects.With(new CardData.StatusEffectStacks(effect, new Vector2Int(min, max).Random()));
        }
    }
}
