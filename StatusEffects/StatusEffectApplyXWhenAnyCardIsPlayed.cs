using System.Collections;
using System.Collections.Generic;

namespace Spirefrost
{

    // This now only works for items, but thats my only use case at the moment
    public class StatusEffectApplyXWhenAnyCardIsPlayed : StatusEffectApplyX
    {
        public TargetConstraint[] triggerConstraints;

        public bool targetPlayedCard;

        public bool worksInHand;

        private bool primed;

        private Entity entityRef;

        private Entity[] targetsRef;

        public override void Init()
        {
            Events.OnActionPerform += CheckAction;
        }

        public void OnDestroy()
        {
            Events.OnActionPerform -= CheckAction;
        }

         private void CheckAction(PlayAction playAction)
        {
            if (playAction is ActionReduceUses && primed)
            {
                ActionQueue.Stack(new ActionSequence(DoStuff(entityRef, targetsRef))
                {
                    note = "StatusEffectApplyXWhenAnyCardIsPlayed"
                }, fixedPosition: true);
            }
        }

        public static CardContainer[] GetWasInRows(Entity entity, IEnumerable<Entity> targets)
        {
            if (entity.data.playType == Card.PlayType.Play && entity.NeedsTarget)
            {
                HashSet<CardContainer> list = new HashSet<CardContainer>();
                foreach (Entity target in targets)
                {
                    if (target.containers != null && target.containers.Length != 0)
                    {
                        list.AddRange(target.containers);
                    }
                    else
                    {
                        list.AddRange(target.preContainers);
                    }
                }

                return list.ToArray();
            }

            return entity.containers;
        }

        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            if (target.enabled && !primed && ((worksInHand && References.Player.handContainer.Contains(target)) || Battle.IsOnBoard(target)))
            {
                foreach (TargetConstraint triggerConstraint in triggerConstraints)
                {
                    if (!triggerConstraint.Check(entity))
                    {
                        return false;
                    }
                }

                primed = true;
                entityRef = entity;
                targetsRef = targets;
            }

            return false;
        }

        public IEnumerator DoStuff(Entity entity, Entity[] targets)
        {
            primed = false;
            entityRef = null;
            targetsRef = null;

            if (targetPlayedCard)
            {
                return Run(new List<Entity>() { entity });
            }
            else
            {
                return Run(GetTargets(null, GetWasInRows(entity, targets), null, targets));
            }
        }
    }
}
