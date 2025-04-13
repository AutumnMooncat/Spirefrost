using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spirefrost
{
    public class TargetConstraintEmptyPile : TargetConstraint
    {
        public enum PileType
        {
            Draw,
            Hand,
            Discard
        }

        public PileType pile;

        public override bool Check(Entity target)
        {
            if (pile == PileType.Draw)
            {
                CardContainer drawContainer = References.Player.drawContainer;
                if (drawContainer != null && drawContainer.Count > 0)
                {
                    return not;
                }
                return !not;
            }

            if (pile == PileType.Hand)
            {
                CardContainer handContainer = References.Player.handContainer;
                if (handContainer != null && handContainer.Count > 0)
                {
                    return not;
                }
                return !not;
            }

            if (pile == PileType.Discard)
            {
                CardContainer discardContainer = References.Player.discardContainer;
                if (discardContainer != null && discardContainer.Count > 0)
                {
                    return not;
                }
                return !not;
            }

            return false;
        }

        public override bool Check(CardData targetData)
        {
            return Check((Entity)null);
        }
    }

    public class ScriptableSkillsInHand : ScriptableAmount
    {
        public override int Get(Entity entity)
        {
            return References.Player.handContainer.Where(e => e.data.IsItem && !e.data.hasAttack).Count();
        }
    }
}
