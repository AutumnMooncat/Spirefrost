using Spirefrost.Patches;
using System;
using System.Collections;
using UnityEngine;

namespace Spirefrost
{
    internal class ArbitraryExecution : PlayAction
    {
        public readonly Routine routine;

        public delegate void ToRun();

        public ArbitraryExecution(ToRun toRun)
        {
            routine = new Routine(RoutineRunnable(toRun), autoStart: false);
        }

        private IEnumerator RoutineRunnable(ToRun runnable)
        {
            runnable();
            yield break;
        }

        public override IEnumerator Run()
        {
            routine.Start();
            while (routine.IsRunning)
            {
                yield return null;
            }
        }
    }

    internal class ActionRefreshPhase : PlayAction
    {
        public readonly Entity entity;

        public readonly CardAnimation animation;

        public readonly bool updateData;

        public readonly float healthFactor;

        public ActionRefreshPhase(Entity entity, CardAnimation animation, bool updateData = false, float healthFactor = 1f)
        {
            this.entity = entity;
            this.animation = animation;
            this.updateData = updateData;
            this.healthFactor = healthFactor;
        }

        public override IEnumerator Run()
        {
            if (!entity.IsAliveAndExists())
            {
                yield break;
            }

            Events.InvokeEntityChangePhase(entity);

            PauseMenu.Block();
            DeckpackBlocker.Block();
            if (Deckpack.IsOpen && References.Player.entity.display is CharacterDisplay characterDisplay)
            {
                characterDisplay.CloseInventory();
            }

            ChangePhaseAnimationSystem animationSystem = UnityEngine.Object.FindObjectOfType<ChangePhaseAnimationSystem>();
            if ((bool)animationSystem)
            {
                yield return animationSystem.Focus(entity);
            }

            if ((bool)animation)
            {
                yield return animation.Routine(entity);
            }

            PlayAction[] actions = ActionQueue.GetActions();
            foreach (PlayAction playAction in actions)
            {
                if (playAction is ActionTrigger actionTrigger && actionTrigger.entity == entity)
                {
                    ActionQueue.Remove(playAction);
                }
            }

            ActionQueue.Stack(new ActionSequence(Refresh(entity))
            {
                note = "Refresh unit phase",
                priority = 10
            }, fixedPosition: true);

            if ((bool)animationSystem)
            {
                ActionQueue.Stack(new ActionSequence(animationSystem.UnFocus())
                {
                    note = "Unfocus unit",
                    priority = 10
                }, fixedPosition: true);
            }
        }

        public IEnumerator Refresh(Entity entity)
        {
            entity.alive = false;
            if (updateData)
            {
                if (healthFactor != 1f)
                {
                    LifeLinkPatch.tracked[entity] = healthFactor;
                }
                yield return entity.ClearStatuses();
                yield return entity.display.UpdateData(doPing: true);
            } 
            else
            {
                if (healthFactor != 1f)
                {
                    int newHealth = healthFactor >= 1f ? Mathf.CeilToInt(entity.hp.max * healthFactor) : Mathf.FloorToInt(entity.hp.max * healthFactor);
                    entity.hp.current = Math.Max(1, newHealth);
                    entity.hp.max = Math.Max(entity.hp.max, newHealth);
                }
                else
                {
                    entity.hp.current = entity.hp.max;
                }
                yield return entity.display.UpdateDisplay(doPing: true);
            }
            entity.alive = true;
            yield return StatusEffectSystem.EntityEnableEvent(entity);
        }
    }
}
