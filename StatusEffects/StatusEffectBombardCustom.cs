using Spirefrost.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectBombardCustom : StatusEffectBombard
    {
        public override void Init()
        {
            base.Init();
            OnCardMove += BoardCheck;
            Events.OnEntityTrigger += EntityTrigger2;
        }

        public new void OnDestroy()
        {
            base.OnDestroy();
            Events.OnEntityTrigger -= EntityTrigger2;
        }

        public IEnumerator BoardCheck(Entity entity)
        {
            if (entity == target && MovedToBoard(target) && !BombardPatches.IsTracked(target))
            {
                yield return SetTargets();
            }
        }

        private bool MovedToBoard(Entity entity)
        {
            return Battle.IsOnBoard(entity) && !Battle.IsOnBoard(entity.preContainers);
        }

        public void EntityTrigger2(ref Trigger trigger)
        {
            if (trigger.entity == target && CanTrigger() && trigger.type == "smackback")
            {
                trigger = new TriggerBombard(trigger.entity, trigger.triggeredBy, "bombard", trigger.targets, targetList.ToArray());
            }
        }
    }
}
