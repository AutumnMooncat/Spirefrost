using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectApplyXWhenAlliesAttackOnBoard : StatusEffectApplyXWhenAlliesAttack
    {
        public override bool RunPreAttackEvent(Hit hit)
        {
            return base.RunPreAttackEvent(hit) && Battle.IsOnBoard(target) && Battle.IsOnBoard(hit.attacker);
        }
    }
}
