using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Spirefrost
{
    internal class SpirefrostUtils
    {
        internal static event UnityAction<Entity> OnMovedByDiscarder;

        internal static void InvokeMovedByDiscarder(Entity entity)
        {
            UnityAction<Entity> onMovedByDiscarder = OnMovedByDiscarder;
            if (onMovedByDiscarder == null)
            {
                return;
            }
            onMovedByDiscarder(entity);
        }

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

        internal static IEnumerator DuplicationLogic(CardUpgradeData dupeData, CardData cardData)
        {
            //Create Card Clone
            CardData cloneData = cardData.Clone(runCreateScripts: false);
            cloneData.upgrades.RemoveAll((CardUpgradeData a) => a.type == CardUpgradeData.Type.Crown);
            dupeData.Clone().Assign(cloneData);

            // Add Data To Inventory
            Debug.Log("Duplication Potion → adding [" + cloneData.name + "] to " + References.Player.name + "'s deck");
            References.Player.data.inventory.deck.Add(cloneData);

            // Add To Sequence
            GameObject deckDisplayObject = GameObject.Find("Canvas/Padding/PlayerDisplay/DeckDisplay");
            DeckDisplaySequence sequence = deckDisplayObject.GetComponent<DeckDisplaySequence>();
            Card card = CardManager.Get(cloneData, sequence.activeCardsGroup.cardController, null, inPlay: false, isPlayerCard: true);
            sequence.activeCardsGroup.AddCard(card);

            // Update Card
            Routine.Clump clump = new Routine.Clump();
            clump.Add(card.UpdateData());
            //clump.Add(Sequences.Wait(10));
            yield return clump.WaitForEnd();
            sequence.UpdatePositions();

            // Invoke Enter Backpack
            Events.InvokeEntityEnterBackpack(card.entity);
            
            CardContainer[] containers = card.entity.containers;
            for (int i = 0; i < containers.Length; i++)
            {
                containers[i].TweenChildPositions();
            }

            // Remove Lock
            MainModFile.instance.looping = false;
        }
    }
}
