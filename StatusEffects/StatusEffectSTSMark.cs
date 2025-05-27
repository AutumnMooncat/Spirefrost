using Spirefrost.Builders.Icons;
using System.Collections;

namespace Spirefrost.StatusEffects
{
    public class StatusEffectSTSMark : StatusEffectData
    {
        public override void Init()
        {
            OnStack += DoStuff;
        }

        private IEnumerator DoStuff(int stacks)
        {
            // Dont Trigger On Reload Battle
            if (BattleSaveSystem.instance.loading)
            {
                yield break;
            }

            // All enemies with Mark lose hp
            foreach (Entity entity in Battle.GetAllUnits(target.owner))
            {
                int markAmount = 0;
                foreach (StatusEffectData effect in entity.statusEffects)
                {
                    if (effect is StatusEffectSTSMark && effect.count > 0)
                    {
                        markAmount += effect.count;
                        
                    }
                }
                if (markAmount > 0)
                {
                    // Hit em
                    Hit hit = new Hit(GetDamager(), entity, markAmount)
                    {
                        screenShake = 0.25f,
                        canRetaliate = false,
                        damageType = MarkIcon.DamageID
                    };
                    yield return hit.Process();
                    yield return Sequences.Wait(0.2f);
                }
            }
        }
    }
}
