using System;
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
            Events.OnEntityTrigger += EntityTrigger2;
        }

        public new void OnDestroy()
        {
            base.OnDestroy();
            Events.OnEntityTrigger -= EntityTrigger2;
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
