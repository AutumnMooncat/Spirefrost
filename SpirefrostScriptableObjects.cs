using Spirefrost.Patches;
using System.Linq;
using UnityEngine;

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

    public class TargetConstraintLastInHand : TargetConstraint
    {
        public override bool Check(Entity target)
        {
            if (target.InHand() && target.owner.handContainer.FirstOrDefault((Entity a) => a.alive) == target)
            {
                return !not;
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
        private readonly TargetModeRow barrage = CreateInstance<TargetModeRow>();

        public bool doesDamage;

        public override bool Check(Entity target)
        {
            if (relevantEntity == null)
            {
                MainModFile.Print($"OwnerRelevantTargetConstraint owner was null");
                return false;
            }
            if (doesDamage && !target.canBeHit)
            {
                return false;
            }

            BarragePatch.shortCircuit = true;
            Entity[] validTargets = barrage.GetTargets(relevantEntity, null, null);
            BarragePatch.shortCircuit = false;
            if (validTargets != null)
            {
                if (validTargets.Contains(target) && barrage.CanTarget(target))
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

    public class TargetConstraintPseudoFrontEnemy : OwnerRelevantTargetConstraint
    {
        private readonly TargetModeBasic basic = CreateInstance<TargetModeBasic>();

        public bool doesDamage;

        public override bool Check(Entity target)
        {
            if (relevantEntity == null)
            {
                MainModFile.Print($"OwnerRelevantTargetConstraint owner was null");
                return false;
            }
            if (doesDamage && !target.canBeHit)
            {
                return false;
            }

            Entity[] validTargets = basic.GetTargets(relevantEntity, null, null);
            if (validTargets != null && validTargets.Contains(target))
            {
                return !not;
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

    public class ScriptableAttacksInHand : ScriptableAmount
    {
        public override int Get(Entity entity)
        {
            return References.Player.handContainer.Where(e => e.data.IsItem && e.data.hasAttack).Count();
        }
    }

    public class ScriptableMissingHealth : ScriptableAmount
    {
        public float multiplier = 1f;

        public bool roundUp;

        public override int Get(Entity entity)
        {
            if (!entity || !entity.data.hasHealth)
            {
                return 0;
            }

            return Mult(entity.hp.max - entity.hp.current);
        }

        public int Mult(int amount)
        {
            if (!roundUp)
            {
                return Mathf.FloorToInt((float)amount * multiplier);
            }

            return Mathf.RoundToInt((float)amount * multiplier);
        }
    }
}
