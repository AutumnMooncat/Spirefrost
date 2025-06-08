using Spirefrost.Builders.Icons;
using System.Collections;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectSTSConstricted : StatusEffectData
    {
        public bool subbed;

        public bool primed;

        public override void Init()
        {
            OnTurnEnd += DealDamage;
            Events.OnPostProcessUnits += Prime;
            subbed = true;
        }

        public void OnDestroy()
        {
            Unsub();
        }

        public void Unsub()
        {
            if (subbed)
            {
                Events.OnPostProcessUnits -= Prime;
                subbed = false;
            }
        }

        public void Prime(Character character)
        {
            primed = true;
            Unsub();
        }

        public override bool RunTurnEndEvent(Entity entity)
        {
            if (primed && target.enabled)
            {
                return entity == target;
            }

            return false;
        }

        public IEnumerator DealDamage(Entity entity)
        {
            Hit hit = new Hit(GetDamager(), target, count)
            {
                screenShake = 0.25f,
                damageType = ConstrictedIcon.DamageID
            };
            yield return hit.Process();
            yield return Sequences.Wait(0.2f);
        }
    }
}
