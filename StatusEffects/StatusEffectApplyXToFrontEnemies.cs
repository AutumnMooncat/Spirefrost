using System.Collections;
using System.Collections.Generic;

namespace Spirefrost.StatusEffects
{
    public class StatusEffectApplyXToFrontEnemies : StatusEffectApplyX
    {
        public override void Init()
        {
            OnCardPlayed += Run;
        }

        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            return entity == target;
        }

        public IEnumerator Run(Entity entity, Entity[] targets)
        {
            int a = GetAmount();
            List<Entity> toAffect = new List<Entity>();
            foreach (CardContainer row in Battle.instance.GetRows(Battle.GetOpponent(target.owner)))
            {
                toAffect.AddIfNotNull(row.GetTop());
            }

            if (toAffect.Count <= 0)
            {
                yield break;
            }

            target.curveAnimator.Ping();
            yield return Sequences.Wait(0.13f);
            Routine.Clump clump = new Routine.Clump();
            foreach (Entity item in toAffect)
            {
                Hit hit = new Hit(target, item, 0);
                hit.AddStatusEffect(effectToApply, a);
                clump.Add(hit.Process());
            }

            yield return clump.WaitForEnd();
            yield return Sequences.Wait(0.13f);
        }
    }
}
