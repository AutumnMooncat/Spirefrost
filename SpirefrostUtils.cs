using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Spirefrost
{
    internal class SpirefrostUtils
    {
        internal class AutoAdd
        {

            [AttributeUsage(AttributeTargets.Class)]
            internal class Ignore : Attribute { }

            private IEnumerable<Type> GetAll(Type type)
            {
                return AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(t => t.IsSubclassOf(type) && !Attribute.IsDefined(t, typeof(Ignore)));
            }

            internal List<object> Process(Type type, string methodName)
            {
                List<object> result = new List<object>();
                foreach (Type t in GetAll(type))
                {
                    object obj = (t.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic)?.Invoke(null, null)) ?? throw new Exception($"AutoAdd Error: Type {t} does not define static method {methodName}");
                    result.Add(obj);
                }
                return result;
            }
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
