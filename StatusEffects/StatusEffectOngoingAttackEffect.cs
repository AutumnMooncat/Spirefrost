using System.Collections;
using System.Linq;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectOngoingAttackEffect : StatusEffectOngoing
    {
        public StatusEffectData effect;

        private bool manuallyAdded;

        public override object GetMidBattleData()
        {
            return manuallyAdded;
        }

        public override void RestoreMidBattleData(object data)
        {
            if (data is bool b)
            {
                manuallyAdded = b;
            }
        }

        public override IEnumerator Add(int add)
        {
            CardData.StatusEffectStacks found = target.attackEffects.FirstOrDefault(stack => stack.data == effect);
            if (found != null)
            {
                found.count += add;
            }
            else
            {
                manuallyAdded = true;
                target.attackEffects.Add(new CardData.StatusEffectStacks(effect, add));
            }

            if (target.display is Card card)
            {
                card.promptUpdateDescription = true;
            }
            target.PromptUpdate();
            yield break;
        }

        public override IEnumerator Remove(int remove)
        {
            CardData.StatusEffectStacks found = target.attackEffects.FirstOrDefault(stack => stack.data == effect);
            if (found != null)
            {
                found.count -= remove;
                if (manuallyAdded && found.count <= 0)
                {
                    target.attackEffects.Remove(found);
                }
            }
            
            if (target.display is Card card)
            {
                card.promptUpdateDescription = true;
            }
            target.PromptUpdate();
            yield break;
        }
    }
}
