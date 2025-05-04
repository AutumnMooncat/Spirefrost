using System.Collections;
using System.Linq;

namespace Spirefrost
{
    public class StatusEffectCopyAttackEffectsPreTrigger : StatusEffectData
    {
        public bool copyAttackValue;

        public override void Init()
        {
            base.PreTrigger += EntityPreTrigger;
        }

        public override bool RunPreTriggerEvent(Trigger trigger)
        {
            return CheckTrigger(trigger);
        }

        private bool CheckTrigger(Trigger trigger)
        {
            if (!target.enabled || trigger.entity != target || target.silenced)
            {
                return false;
            }

            if (trigger.targets.Count() == 0)
            {
                return false;
            }

            return true;
        }

        private IEnumerator EntityPreTrigger(Trigger trigger)
        {
            foreach (var item in trigger.targets)
            {
                if (copyAttackValue)
                {
                    target.damage.current += item.damage.current + item.tempDamage.Value;
                }

                foreach (var effect in item.attackEffects)
                {
                    CardData.StatusEffectStacks toApply = target.attackEffects.Find((CardData.StatusEffectStacks a) => a.data.name == effect.data.name);
                    if (toApply != null)
                    {
                        toApply.count += effect.count;
                    } 
                    else
                    {
                        target.attackEffects.Add(effect.Clone()); 
                    }
                }
            }
            if (target.display is Card card)
            {
                card.promptUpdateDescription = true;
            }
            yield return Remove();
        }
    }
}
