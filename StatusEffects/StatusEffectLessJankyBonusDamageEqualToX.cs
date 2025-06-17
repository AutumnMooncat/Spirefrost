using System.Collections;
using System.Linq;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectLessJankyBonusDamageEqualToX : StatusEffectData
    {
        public enum On
        {
            Self,
            Board,
            ScriptableAmount
        }

        public On on;

        public ScriptableAmount scriptableAmount;

        public bool add = true;

        public bool scaleByCount;

        public bool health;

        public string effectType = "shell";

        public bool ping = true;

        public int currentAmount;

        public bool toReset;

        public bool UseScriptableAmount => on == On.ScriptableAmount;

        public override void Init()
        {
            PreCardPlayed += Gain;
            OnActionPerformed += ActionPerformed;
        }

        public override bool RunPreCardPlayedEvent(Entity entity, Entity[] targets)
        {
            if (entity == target)
            {
                return CanTrigger();
            }

            return false;
        }

        public IEnumerator Gain(Entity entity, Entity[] targets)
        {
            int num = Find();
            if (scaleByCount)
            {
                num *= GetAmount();
            }
            if (!toReset || num != currentAmount)
            {
                if (toReset)
                {
                    LoseCurrentAmount();
                }

                if (num > 0)
                {
                    yield return GainAmount(num);
                }
            }
        }

        public IEnumerator GainAmount(int amount)
        {
            toReset = true;
            int value = target.tempDamage.Value;
            if (add)
            {
                target.tempDamage += amount;
            }
            else
            {
                target.tempDamage.Value = amount;
            }

            currentAmount = target.tempDamage.Value - value;
            target.PromptUpdate();
            if (ping)
            {
                target.curveAnimator.Ping();
                yield return Sequences.Wait(0.5f);
            }
        }

        public override bool RunActionPerformedEvent(PlayAction action)
        {
            if (toReset)
            {
                return ActionQueue.Empty;
            }

            return false;
        }

        public IEnumerator ActionPerformed(PlayAction action)
        {
            LoseCurrentAmount();
            yield break;
        }

        public void LoseCurrentAmount()
        {
            toReset = false;
            if (currentAmount != 0)
            {
                target.tempDamage -= currentAmount;
                currentAmount = 0;
                target.PromptUpdate();
            }
        }

        public int Find()
        {
            switch (on)
            {
                case On.Self:
                    return FindOnSelf();
                case On.Board:
                    return FindOnBoard();
                case On.ScriptableAmount:
                    return scriptableAmount.Get(target);
                default:
                    return 0;
            }
        }

        public int FindOnSelf()
        {
            int result = 0;
            if (health)
            {
                result = target.hp.current;
            }
            else
            {
                StatusEffectData statusEffectData = target.FindStatus(effectType);
                if ((bool)statusEffectData && statusEffectData.count > 0)
                {
                    result = statusEffectData.count;
                }
            }

            return result;
        }

        public int FindOnBoard()
        {
            int num = 0;
            if (health)
            {
                return num + Battle.GetCardsOnBoard().Sum((Entity e) => target.hp.current);
            }

            return num + (from entity in Battle.GetCardsOnBoard()
                          select entity.FindStatus(effectType) into effect
                          where (bool)effect && effect.count > 0
                          select effect).Sum((StatusEffectData effect) => effect.count);
        }
    }
}
