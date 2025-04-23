using Deadpan.Enums.Engine.Components.Modding;
using System.Collections;
using UnityEngine;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.CardUpgrades
{
    [ToPoolList(PoolListType.Charms)]
    internal class DuplicationPotion : SpirefrostBuilder
    {
        internal static string ID => "DuplicationPotionCharm";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardUpgradeDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/DuplicationCharm.png")
                .WithTitle("Duplication Potion")
                .WithText($"Create a copy of an <Item>") // \nDoes not take up a charm slot
                .WithTier(2)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    CardScriptRunnable duplicationScript = ScriptableObject.CreateInstance<CardScriptRunnable>();
                    duplicationScript.runnable = card =>
                    {
                        if (!MainModFile.instance.looping)
                        {
                            // Safety Lock
                            MainModFile.instance.looping = true;

                            IEnumerator logic = DuplicationLogic(data, card);

                            while (logic.MoveNext())
                            {
                                object n = logic.Current;
                                Debug.Log("Duplication Script: " + n);
                            }
                        }
                    };
                    data.scripts = new CardScript[]
                    {
                        duplicationScript
                    };
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintIsItem>()
                    };
                    //data.takeSlot = false;
                });
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
