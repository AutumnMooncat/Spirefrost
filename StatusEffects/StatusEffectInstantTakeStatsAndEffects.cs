using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectInstantTakeStatsAndEffects : StatusEffectInstant
    {
        public bool gainHealth = true;

        public bool gainAttack = true;

        public bool gainEffects = true;

        public TraitData[] illegalTraits;

        public StatusEffectData[] illegalEffects;

        public override IEnumerator Process()
        {
            if ((bool)applier && applier.alive && (bool)target && (gainHealth || gainAttack || gainEffects))
            {
                if (gainHealth)
                {
                    GainHealth();
                }

                if (gainAttack)
                {
                    GainAttack();
                }

                if (gainEffects)
                {
                    yield return GainEffects();
                }

                applier.PromptUpdate();
            }

            yield return base.Process();
        }

        public void GainHealth()
        {
            applier.hp.current += target.hp.current;
            applier.hp.max += target.hp.max;
        }

        public void GainAttack()
        {
            applier.damage.current += target.damage.current;
            applier.damage.max += target.damage.max;
        }

        public IEnumerator GainEffects()
        {
            applier.attackEffects = CardData.StatusEffectStacks.Stack(applier.attackEffects, target.attackEffects).ToList();
            List<StatusEffectData> list = target.statusEffects.Where(effect => effect != this && !illegalEffects.Any(illegal => illegal.name == effect.name)).ToList();
            foreach (Entity.TraitStacks trait in target.traits)
            {
                foreach (StatusEffectData passiveEffect in trait.passiveEffects)
                {
                    list.Remove(passiveEffect);
                }

                int num = trait.count - trait.tempCount;
                if (num > 0 && !illegalTraits.Select((TraitData t) => t.name).Contains(trait.data.name))
                {
                    applier.GainTrait(trait.data, num);
                }
            }

            foreach (StatusEffectData item in list)
            {
                yield return StatusEffectSystem.Apply(applier, target, item, item.count);
            }

            yield return applier.UpdateTraits();
            applier.display.promptUpdateDescription = true;
        }
    }
}
