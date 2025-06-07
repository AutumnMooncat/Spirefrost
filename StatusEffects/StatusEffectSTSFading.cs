using System.Collections;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectSTSFading : StatusEffectData
    {
        private bool active;

        public override void Init()
        {
            Events.OnEntityDisplayUpdated += EntityDisplayUpdated;
            OnTurnEnd += TurnEnd;
            OnEnd += End;
        }

        public void OnDestroy()
        {
            Events.OnEntityDisplayUpdated -= EntityDisplayUpdated;
        }

        private void EntityDisplayUpdated(Entity entity)
        {
            if (target.hp.current <= 0 && entity == target)
            {
                active = true;
                target.hp.current = 0;
            }
        }

        public override bool RunPostHitEvent(Hit hit)
        {
            if (hit.target == target && target.hp.current <= 0)
            {
                active = true;
                target.hp.current = 0;
            }

            return false;
        }

        public override object GetMidBattleData()
        {
            return active;
        }

        public override void RestoreMidBattleData(object data)
        {
            if (data is bool b)
            {
                active = b;
            }
        }

        private IEnumerator End()
        {
            yield return StatusEffectSystem.Apply(target, applier, MainModFile.instance.TryGet<StatusEffectData>("Kill"), 1);
        }

        public override bool RunTurnEndEvent(Entity entity)
        {
            if (target.enabled && entity == target && Battle.IsOnBoard(target) && active)
            {
                return true;
            }

            return false;
        }

        private IEnumerator TurnEnd(Entity entity)
        {
            active = true;
            int amount = 1;
            Events.InvokeStatusEffectCountDown(this, ref amount);
            if (amount != 0)
            {
                yield return CountDown(target, amount);
            }
        }
    }
}
