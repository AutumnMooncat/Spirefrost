using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectFeed : StatusEffectApplyX
    {
        public override void Init()
        {
            base.OnEntityDestroyed += CheckDestroy;
            effectToApply = MainModFile.instance.TryGet<StatusEffectData>("Increase Max Health");
            applyToFlags = ApplyToFlags.Self;
            doPing = true;
        }

        public override bool RunEntityDestroyedEvent(Entity entity, DeathType deathType)
        {
            if (entity.lastHit != null)
            {
                return entity.lastHit.attacker == target;
            }

            return false;
        }

        public IEnumerator CheckDestroy(Entity entity, DeathType deathType)
        {
            int amount = GetAmount();
            if (amount > 0)
            {
                CardData deckVersion = References.PlayerData.inventory.deck.list.Where(data => data.id == target.data.id).FirstOrDefault();
                if (deckVersion)
                {
                    deckVersion.hp += amount;
                    if (deckVersion.hp <= 0)
                    {
                        deckVersion.hp = 1;
                    }
                }
            }
            return Run(new List<Entity>() { target });
        }
    }
}
