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

    public class TargetConstraintFrontUnit : TargetConstraint
    {
        public override bool Check(Entity target)
        {
            foreach (CardContainer row in Battle.instance.GetRows(target.owner))
            {
                if (target == row.GetTop())
                {
                    return !not;
                }
            }
            return not;
        }

        public override bool Check(CardData targetData)
        {
            return false;
        }
    }

    public abstract class OwnerRelevantTargetConstraint : TargetConstraint
    {
        public Entity relevantEntity;
    }

    public class TargetConstraintPseudoBarrage : OwnerRelevantTargetConstraint
    {
        public override bool Check(Entity target)
        {
            if (relevantEntity == null)
            {
                MainModFile.Print($"OwnerRelevantTargetConstraint owner was null");
                return false;
            }

            CardContainer[] relevantRows = relevantEntity.containers;
            int[] relevantIndices = relevantRows.Select(cont => References.Battle.GetRowIndex(cont)).ToArray();
            bool targetingEmptyRow = relevantIndices.Select(i => relevantEntity.GetEnemiesInRow(i)).All(enemies => enemies.Count == 0);

            foreach (CardContainer row in Battle.instance.GetRows(target.owner))
            {
                if (relevantIndices.Contains(References.Battle.GetRowIndex(row)))
                {
                    return !not;
                }
                else
                {
                    if (targetingEmptyRow)
                    {
                        return !not;
                    }
                }
            }

            return not;
        }

        public override bool Check(CardData targetData)
        {
            return false;
        }
    }

    public class TargetConstraintPseudoFrontEnemy : OwnerRelevantTargetConstraint
    {
        public override bool Check(Entity target)
        {
            if (relevantEntity == null)
            {
                MainModFile.Print($"OwnerRelevantTargetConstraint owner was null");
                return false;
            }

            CardContainer[] relevantRows = relevantEntity.containers;
            int[] relevantIndices = relevantRows.Select(cont => References.Battle.GetRowIndex(cont)).ToArray();
            bool targetingEmptyRow = relevantIndices.Select(i => relevantEntity.GetEnemiesInRow(i)).All(enemies => enemies.Count == 0);

            foreach (CardContainer row in Battle.instance.GetRows(target.owner))
            {
                if (target == row.GetTop())
                {
                    if (relevantIndices.Contains(References.Battle.GetRowIndex(row)))
                    {
                        return !not;
                    }
                    else
                    {
                        if (targetingEmptyRow)
                        {
                            return !not;
                        }
                    }
                }
                    
            }
            
            return not;
        }

        public override bool Check(CardData targetData)
        {
            return false;
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
