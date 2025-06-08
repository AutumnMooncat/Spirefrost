using System.Collections;
using System.Linq;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectOngoingAttackEffect : StatusEffectOngoing
    {
        public StatusEffectData effect;

        private int _manuallyAdded;

        public int ManuallyAdded { get => _manuallyAdded; }

        public override IEnumerator Add(int add)
        {
            _manuallyAdded += add;
            CardData.StatusEffectStacks found = target.attackEffects.FirstOrDefault(stack => stack.data == effect);
            if (found != null)
            {
                found.count += add;
            }
            else
            {
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
            _manuallyAdded -= remove;
            CardData.StatusEffectStacks found = target.attackEffects.FirstOrDefault(stack => stack.data == effect);
            if (found != null)
            {
                found.count -= remove;
                if (_manuallyAdded == 0 && found.count <= 0)
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
