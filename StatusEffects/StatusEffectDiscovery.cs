using Spirefrost.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using WildfrostHopeMod.Utils;

namespace Spirefrost.StatusEffects
{
    // Thank you AbsentAbigail!
    public class StatusEffectDiscovery : StatusEffectInstant
    {
        public enum CardSource
        {
            Draw,
            Discard,
            Custom // Use Summon Copy
        }

        public CardSource source = CardSource.Draw;
        public string[] customCardList;
        public int copies = 1;
        public CardData.StatusEffectStacks[] addEffectStacks;
        public bool amountIsForApply;
        public LocalizedString title;

        private CardContainer constructedContainer;
        private GameObject selectCardObject;
        private GameObject selectCardObjectGroup;

        private Entity selectedEntity;
        private CardPocketSequence pocketSequence;

        public override IEnumerator Process()
        {
            pocketSequence = FindObjectOfType<CardPocketSequence>(true);
            CardControllerSelectCard cardController = (CardControllerSelectCard)pocketSequence.cardController;
            cardController.pressEvent.AddListener(ChooseCard);
            cardController.canPress = true;
            CardContainer container = GetCardContainer();

            if (source == CardSource.Custom)
            {
                foreach (Entity entity in container)
                {
                    yield return entity.GetCard().UpdateData();
                    foreach (StatusEffectData data in entity.statusEffects)
                    {
                        if (data is StatusEffectFreeAction free)
                        {
                            // RunBeginEvent fails to create pet, do it manually
                            free.hasEffect = true;
                            if (entity.display is Card card)
                            {
                                card.itemHolderPet?.Create(free.petPrefab);
                                Events.InvokeNoomlinShow(entity);
                            }
                        }
                    }
                }
            }

            CinemaBarSystem.In();
            CinemaBarSystem.SetSortingLayer("UI2");
            if (!title.IsEmpty)
            {
                CinemaBarSystem.Top.SetPrompt(title.GetLocalizedString(), "Select");
            }
            pocketSequence.AddCards(container);

            DiscoveryPatches.instantSnap = true;
            yield return pocketSequence.Run();
            DiscoveryPatches.instantSnap = false;

            if (selectedEntity != null) //Card Selected
            {
                if (source != CardSource.Custom)
                {
                    Events.InvokeCardDraw(1);
                }
                
                yield return Sequences.CardMove(selectedEntity, new CardContainer[] { References.Player.handContainer });
                References.Player.handContainer.TweenChildPositions();

                if (source != CardSource.Custom)
                {
                    Events.InvokeCardDrawEnd();
                }
                    
                selectedEntity.flipper.FlipUp();
                //yield return Sequences.WaitForAnimationEnd(_selected);
                yield return new ActionRunEnableEvent(selectedEntity).Run();
                selectedEntity.display.hover.enabled = true;

                foreach (CardData.StatusEffectStacks stack in addEffectStacks)
                {
                    ActionQueue.Stack(new ActionApplyStatus(selectedEntity, null, stack.data, amountIsForApply ? GetAmount() : stack.count));
                }

                selectedEntity.display.promptUpdateDescription = true;
                selectedEntity.PromptUpdate();

                ActionQueue.Stack(new ActionSequence(selectedEntity.UpdateTraits()) { note = $"[{selectedEntity}] Update Traits" });

                selectedEntity = null;
            }

            constructedContainer?.ClearAndDestroyAllImmediately();

            cardController.canPress = false;
            cardController.pressEvent.RemoveListener(ChooseCard);

            CinemaBarSystem.Clear();
            CinemaBarSystem.Out();

            yield return Remove();
        }

        private void ChooseCard(Entity entity)
        {
            selectedEntity = entity;
            pocketSequence.promptEnd = true;
        }

        private CardContainer GetCardContainer()
        {
            switch (source)
            {
                case CardSource.Draw:
                    return References.Player.drawContainer;
                case CardSource.Discard:
                    return References.Player.discardContainer;
                case CardSource.Custom:
                    selectCardObjectGroup = new GameObject("SelectCardRoutine");
                    selectCardObjectGroup.SetActive(false);
                    selectCardObjectGroup.transform.SetParent(GameObject.Find("Canvas/Padding/HUD/DeckpackLayout").transform.parent.GetChild(0));
                    selectCardObjectGroup.transform.SetAsFirstSibling();

                    selectCardObject = new GameObject("SelectCard");
                    RectTransform rect = selectCardObject.AddComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(7, 2);

                    constructedContainer = CreateCardGrid(selectCardObjectGroup.transform, rect);

                    FillCardContainer(GetAmount());

                    constructedContainer.AssignController(Battle.instance.playerCardController);

                    return constructedContainer;
                default:
                    return null;
            }
        }

        private void FillCardContainer(int amount)
        {
            if (customCardList.Length <= 0)
            {
                PredicateContainer(amount);
                return;
            }

            if (amount == 0)
            {
                amount = customCardList.Length;
            }

            foreach(string cardName in InPettyRandomOrder(customCardList).Take(amount))
            {
                CardData cardData = AddressableLoader.Get<CardData>("CardData", cardName).Clone();
                Card card = CardManager.Get(cardData, Battle.instance.playerCardController, References.Player, true, true);
                constructedContainer.Add(card.entity);
            }
        }

        private void PredicateContainer(int amount)
        {
            Predicate<CardData> predicate = MainModFile.instance.predicateReferences[name];
            List<CardData> validCards = new List<CardData>();
            foreach (RewardPool pool in References.PlayerData.classData.rewardPools)
            {
                if (pool.type != "Charms")
                {
                    foreach (DataFile data in pool.list)
                    {
                        if (data is CardData card && predicate(card))
                        {
                            validCards.Add(card);
                        }
                    }
                }
            }

            int toAdd = Math.Min(amount, validCards.Count);
            for (int i = 0; i < toAdd; i++)
            {
                CardData randomCard = validCards.RandomItem();
                validCards.Remove(randomCard);
                Entity entity = CardManager.Get(randomCard.Clone(), Battle.instance.playerCardController, References.Player, inPlay: true, isPlayerCard: true).entity;
                constructedContainer.Add(entity);
            }
        }

        // Random Order from Pokefrost StatusEffectChangeData
        private static IOrderedEnumerable<T> InPettyRandomOrder<T>(IEnumerable<T> source)
        {
            return source.OrderBy(_ => Dead.PettyRandom.Range(0f, 1f));
        }

        // Card Grid Code by Phan
        private static CardContainerGrid CreateCardGrid(Transform parent, RectTransform bounds = null)
        {
            return CreateCardGrid(parent, new Vector2(2.25f, 3.375f), 5, bounds);
        }

        private static CardContainerGrid CreateCardGrid(Transform parent, Vector2 cellSize, int columnCount, RectTransform bounds = null)
        {
            GameObject gridObj = new GameObject("CardGrid", typeof(RectTransform), typeof(CardContainerGrid));
            gridObj.transform.SetParent(bounds ?? parent);
            gridObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            CardContainerGrid grid = gridObj.GetComponent<CardContainerGrid>();
            grid.holder = grid.GetComponent<RectTransform>();
            grid.onAdd = new UnityEventEntity(); // Fix null reference
            grid.onAdd.AddListener(entity => entity.flipper.FlipUp()); // Flip up card when it's time (without waiting for others)
            grid.onRemove = new UnityEventEntity(); // Fix null reference

            grid.cellSize = cellSize;
            grid.columnCount = columnCount;

            AddScrollers(gridObj); // No click-and-drag. That needs Scroll View
            Scroller scroller = gridObj.GetOrAdd<Scroller>();
            scroller.bounds = bounds; // Change scroller.bounds here if it only scrolls partially

            return grid;
        }

        private static void AddScrollers(GameObject parentObject)
        {
            Scroller scroller = parentObject.GetOrAdd<Scroller>(); // Scroll with mouse
            parentObject.GetOrAdd<ScrollToNavigation>().scroller = scroller; // Scroll with controllers
            parentObject.GetOrAdd<TouchScroller>().scroller = scroller; // Scroll with touchscreen
        }
    }
}
