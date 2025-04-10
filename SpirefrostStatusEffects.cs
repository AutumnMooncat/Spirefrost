﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using static Routine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using UnityEngine.Localization;
using WildfrostHopeMod.Utils;
using DeadExtensions;

namespace Spirefrost
{
    public class StatusEffectSTSVulnerable : StatusEffectData
    {
        public int amountToClear;

        public StatusEffectSTSVulnerable()
        {
            removeOnDiscard = true;
        }

        public override void Init()
        {
            base.OnHit += MultiplyHit;
        }

        public override bool RunHitEvent(Hit hit)
        {
            if (hit.Offensive && count > 0 && hit.damage > 0)
            {
                return hit.target == target;
            }

            return false;
        }

        public IEnumerator MultiplyHit(Hit hit)
        {
            amountToClear = GetAmount();
            hit.damage = Mathf.CeilToInt(hit.damage * (1 + (amountToClear * 0.5f)));
            ActionQueue.Stack(new ActionSequence(Clear(amountToClear))
            {
                fixedPosition = true,
                note = "Clear Vulnerable"
            });
            yield break;
        }

        public IEnumerator Clear(int clearMe)
        {
            if ((bool)this && (bool)target && target.alive)
            {
                int amount = clearMe;
                Events.InvokeStatusEffectCountDown(this, ref amount);
                if (amount != 0)
                {
                    yield return CountDown(target, amount);
                }
            }
        }
    }

    public class StatusEffectSTSWeakness : StatusEffectData
    {
        public bool cardPlayed;

        public StatusEffectSTSWeakness()
        {
            eventPriority = -999999;
            removeOnDiscard = true;
        }

        public override void Init()
        {
            base.OnActionPerformed += ActionPerformed;
            base.OnHit += HalveDamage;
        }

        public override bool RunHitEvent(Hit hit)
        {
            if (hit.attacker == target && hit.countsAsHit)
            {
                return true;
            }

            return false;
        }

        private IEnumerator HalveDamage(Hit hit)
        {
            hit.damage -= Mathf.CeilToInt(hit.damage / 2f);
            yield break;
        }

        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            if (!cardPlayed && entity == target && count > 0 && targets != null && targets.Length != 0)
            {
                cardPlayed = true;
            }

            return false;
        }

        public override bool RunActionPerformedEvent(PlayAction action)
        {
            if (cardPlayed)
            {
                return ActionQueue.Empty;
            }

            return false;
        }

        public IEnumerator ActionPerformed(PlayAction action)
        {
            cardPlayed = false;
            yield return Clear(1);
        }

        public IEnumerator Clear(int amount)
        {
            Events.InvokeStatusEffectCountDown(this, ref amount);
            if (amount != 0)
            {
                yield return CountDown(target, amount);
            }
        }
    }

    public class StatusEffectSTSAmplify : StatusEffectData
    {
        public bool cardPlayed;

        public int current;

        public int amountToClear;

        public override void Init()
        {
            base.OnActionPerformed += ActionPerformed;
        }

        public override bool RunStackEvent(int stacks)
        {
            current += stacks;
            target.effectBonus += stacks;
            target.PromptUpdate();
            return false;
        }

        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            if (!cardPlayed && entity == target && count > 0)
            {
                cardPlayed = true;
                amountToClear = current;
            }

            return false;
        }

        public override bool RunActionPerformedEvent(PlayAction action)
        {
            if (cardPlayed)
            {
                return ActionQueue.Empty;
            }

            return false;
        }

        public IEnumerator ActionPerformed(PlayAction action)
        {
                cardPlayed = false;
                yield return Clear(amountToClear);
        }

        public IEnumerator Clear(int amount)
        {
            int amount2 = amount;
            Events.InvokeStatusEffectCountDown(this, ref amount2);
            if (amount2 != 0)
            {
                current -= amount2;
                target.effectBonus -= amount2;
                target.PromptUpdate();
                yield return CountDown(target, amount2);
            }
        }

        public override bool RunEndEvent()
        {
            target.effectBonus -= current;
            target.PromptUpdate();
            return false;
        }
    }

    public class StatusEffectDoubleTap : StatusEffectWhileActiveX
    {
        public bool cardPlayed;

        public int current;

        public int amountToClear;

        public override void Init()
        {
            base.Init();
            base.OnActionPerformed += ActionPerformed;
        }

        public override bool RunStackEvent(int stacks)
        {
            current += stacks;
            return false;
        }

        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            if (!cardPlayed && entity == target && count > 0)
            {
                cardPlayed = true;
                amountToClear = current;
            }

            return false;
        }

        public override bool RunActionPerformedEvent(PlayAction action)
        {
            if (cardPlayed)
            {
                return ActionQueue.Empty;
            }

            return false;
        }

        public IEnumerator ActionPerformed(PlayAction action)
        {
            cardPlayed = false;
            yield return Clear(amountToClear);
        }

        public IEnumerator Clear(int amount)
        {
            int amount2 = amount;
            Events.InvokeStatusEffectCountDown(this, ref amount2);
            if (amount2 != 0)
            {
                current -= amount2;
                yield return CountDown(target, amount2);
            }
        }
    }

    public class StatusEffectSTSFlight : StatusEffectData
    {
        public override void Init()
        {
            base.OnHit += FlightHit;
        }

        public override bool RunHitEvent(Hit hit)
        {
            if (hit.Offensive && count > 0 && hit.damage > 0)
            {
                return hit.target == target;
            }

            return false;
        }

        public IEnumerator FlightHit(Hit hit)
        {
            hit.damage = Mathf.RoundToInt(hit.damage / 2f);
            ActionQueue.Stack(new ActionSequence(CountDown())
            {
                fixedPosition = true,
                note = "Count Down Flight"
            });
            yield break;
        }

        public IEnumerator CountDown()
        {
            if ((bool)this && (bool)target && target.alive)
            {
                int amount = 1;
                Events.InvokeStatusEffectCountDown(this, ref amount);
                if (amount != 0)
                {
                    yield return CountDown(target, amount);
                }
            }
        }
    }

    public class StatusEffectSTSMark : StatusEffectData
    {
        public override void Init()
        {
            base.OnStack += DoStuff;
        }

        private IEnumerator DoStuff(int stacks)
        {
            // All enemies with Mark lose hp
            foreach (Entity entity in Battle.GetAllUnits(Battle.GetOpponent(GetDamager().owner)))
            {
                int markAmount = 0;
                foreach (StatusEffectData effect in entity.statusEffects)
                {
                    if (effect is StatusEffectSTSMark && effect.count > 0)
                    {
                        markAmount += effect.count;
                        
                    }
                }
                if (markAmount > 0)
                {
                    // Hit em
                    Hit hit = new Hit(GetDamager(), entity, markAmount)
                    {
                        screenShake = 0.25f,
                        canRetaliate = false,
                    };
                    // Add VFX Later?
                    //var transform = entity.transform;
                    //VFXMod.instance.VFX.TryPlayEffect("stsmark", transform.position, transform.lossyScale);
                    // Add SFX Later?
                    //VFXMod.instance.SFX.TryPlaySound("stsmark");
                    yield return hit.Process();
                    yield return Sequences.Wait(0.2f);
                }
            }
        }
    }

    public class StatusEffectApplyXAfterTurnAndDecay : StatusEffectApplyX
    {
        public bool subbed;

        public bool primed;

        public override void Init()
        {
            base.OnTurnEnd += PostTurn;
            Events.OnPostProcessUnits += Prime;
            subbed = true;
        }

        public void OnDestroy()
        {
            Unsub();
        }

        public void Unsub()
        {
            if (subbed)
            {
                Events.OnPostProcessUnits -= Prime;
                subbed = false;
            }
        }

        public void Prime(Character character)
        {
            primed = true;
            Unsub();
        }

        public override bool RunTurnEndEvent(Entity entity)
        {
            if (primed && target.enabled && Battle.IsOnBoard(target))
            {
                return entity == target;
            }

            return false;
        }

        public IEnumerator PostTurn(Entity entity)
        {
            ActionQueue.Stack(new ActionSequence(CountDown())
            {
                fixedPosition = true,
                note = "Decay After Turn"
            });
            yield return Run(GetTargets());
        }

        public IEnumerator CountDown()
        {
            if ((bool)this && (bool)target && target.alive)
            {
                int amount = 1;
                Events.InvokeStatusEffectCountDown(this, ref amount);
                if (amount != 0)
                {
                    yield return CountDown(target, amount);
                }
            }
        }
    }

    public class StatusEffectApplyXWhenHitOnce : StatusEffectApplyXWhenHit
    {
        public override void Init()
        {
            base.PostHit += RemoveMe;
        }

        public IEnumerator RemoveMe(Hit hit)
        {
            yield return Run(GetTargets(hit, GetTargetContainers(), GetTargetActualContainers()), hit.damage + hit.damageBlocked);
            ActionQueue.Stack(new ActionSequence(CountDown())
            {
                fixedPosition = true,
                note = "Remove Apply When Hit Once"
            });
            yield break;
        }

        public IEnumerator CountDown()
        {
            if ((bool)this && (bool)target && target.alive)
            {
                int amount = GetAmount();
                Events.InvokeStatusEffectCountDown(this, ref amount);
                if (amount != 0)
                {
                    yield return CountDown(target, amount);
                }
            }
        }
    }

    public class StatusEffectApplyXToFrontEnemies : StatusEffectApplyX
    {
        public override void Init()
        {
            base.OnCardPlayed += Run;
        }

        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            return entity == target;
        }

        public IEnumerator Run(Entity entity, Entity[] targets)
        {
            int a = GetAmount();
            List<Entity> toAffect = new List<Entity>();
            foreach (CardContainer row in Battle.instance.GetRows(Battle.GetOpponent(target.owner)))
            {
                toAffect.AddIfNotNull(row.GetTop());
            }

            if (toAffect.Count <= 0)
            {
                yield break;
            }

            target.curveAnimator.Ping();
            yield return Sequences.Wait(0.13f);
            Routine.Clump clump = new Routine.Clump();
            foreach (Entity item in toAffect)
            {
                Hit hit = new Hit(target, item, 0);
                hit.AddStatusEffect(effectToApply, a);
                clump.Add(hit.Process());
            }

            yield return clump.WaitForEnd();
            yield return Sequences.Wait(0.13f);
        }
    }

    public class StatusEffectApplyXToFrontEnemiesWhenHit : StatusEffectApplyX
    {
        public TargetConstraint[] attackerConstraints;

        public override void Init()
        {
            base.PostHit += CheckHit;
        }

        public override bool RunPostHitEvent(Hit hit)
        {
            if (target.enabled && hit.target == target && hit.canRetaliate && (!targetMustBeAlive || (target.alive && Battle.IsOnBoard(target))) && hit.Offensive && hit.BasicHit)
            {
                return CheckAttackerConstraints(hit.attacker);
            }

            return false;
        }

        public IEnumerator CheckHit(Hit hit)
        {
            List<Entity> toAffect = new List<Entity>();
            foreach (CardContainer row in Battle.instance.GetRows(Battle.GetOpponent(target.owner)))
            {
                toAffect.AddIfNotNull(row.GetTop());
            }
            return Run(toAffect);
        }

        public bool CheckAttackerConstraints(Entity attacker)
        {
            if (attackerConstraints != null)
            {
                TargetConstraint[] array = attackerConstraints;
                for (int i = 0; i < array.Length; i++)
                {
                    if (!array[i].Check(attacker))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    public class StatusEffectInstantIncreaseCounter : StatusEffectInstant
    {
        public override IEnumerator Process()
        {
            target.counter.current += GetAmount();
            target.PromptUpdate();
            yield return base.Process();
        }
    }

    public class StatusEffectApplyXWhenAnyCardIsPlayed : StatusEffectApplyX
    {
        public TargetConstraint[] triggerConstraints;
        public Boolean targetPlayedCard;

        public override void Init()
        {
            base.OnCardPlayed += Check;
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
            if (target.enabled)
            {
                foreach (TargetConstraint triggerConstraint in triggerConstraints)
                {
                    if (!triggerConstraint.Check(entity))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public IEnumerator Check(Entity entity, Entity[] targets)
        {
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

    public class StatusEffectConvertXToYPreTrigger : StatusEffectApplyXPreTrigger
    {
        public enum Conversion
        {
            Health,
            Attack,
            Scrap,
        }

        public Conversion toConvert;

        private int oldTemp;

        private bool cardPlayed;

        public StatusEffectConvertXToYPreTrigger()
        {
            scriptableAmount = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
            ((ScriptableFixedAmount)scriptableAmount).amount = 0;
            eventPriority = -10000;
        }

        public override void Init()
        {
            base.PreTrigger += EntityPreTrigger;
            base.OnActionPerformed += ActionPerformed;
        }

        public new IEnumerator EntityPreTrigger(Trigger trigger)
        {
            if (oncePerTurn)
            {
                hasRunThisTurn = true;
            }

            running = true;
            ((ScriptableFixedAmount)scriptableAmount).amount = 0;
            int reduceTo = GetAmount();
            bool prompt = false;
            switch (toConvert)
            {
                case Conversion.Health:
                    if (target.hp.current > reduceTo)
                    {
                        ((ScriptableFixedAmount)scriptableAmount).amount = target.hp.current - reduceTo;
                        target.hp.current = reduceTo;
                        prompt = true;
                    }

                    if (target.hp.max > reduceTo)
                    {
                        target.hp.max = reduceTo;
                        prompt = true;
                    }
                    break;
                case Conversion.Attack:
                    if (target.damage.current + target.tempDamage.Value > reduceTo)
                    {
                        // Current damage goes to reduceTo if higher than it
                        // Temp damage needs to be set so damage+temp = reduceTo
                        // Restore temp later so it doesnt mess stuff up
                        ((ScriptableFixedAmount)scriptableAmount).amount = target.damage.current + target.tempDamage.Value - reduceTo;
                        if (target.damage.current > reduceTo)
                        {
                            target.damage.current = reduceTo;
                            prompt = true;
                        }

                        oldTemp = target.tempDamage.Value;
                        target.tempDamage.Value = reduceTo - target.damage.current;
                    }

                    // Also reduce max if applicable
                    if (target.damage.max > reduceTo)
                    {
                        target.damage.max = reduceTo;
                        prompt = true;
                    }
                    break;
                case Conversion.Scrap:
                    StatusEffectData scrapEffect = target.statusEffects.Find((StatusEffectData a) => a.name.Equals("Scrap"));
                    if (scrapEffect?.count > reduceTo)
                    {
                        if (doPing)
                        {
                            target.curveAnimator.Ping();
                        }
                        count = scrapEffect.count - 1;
                        yield return scrapEffect.RemoveStacks(scrapEffect.count - reduceTo, removeTemporary: false);
                    }
                    break;
            }
            if (prompt)
            {
                target.PromptUpdate();
            }
            if (((ScriptableFixedAmount)scriptableAmount).amount > 0)
            {
                yield return Run(runAgainst);
            }
            runAgainst = null;
            running = false;
        }

        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            if (!cardPlayed && entity == target)
            {
                cardPlayed = true;
            }

            return false;
        }

        public override bool RunActionPerformedEvent(PlayAction action)
        {
            if (cardPlayed)
            {
                return ActionQueue.Empty;
            }

            return false;
        }

        public IEnumerator ActionPerformed(PlayAction action)
        {
            cardPlayed = false;
            target.tempDamage.Value += oldTemp;
            oldTemp = 0;
            yield break;
        }
    }

    public class StatusEffectTempConvertAttackToYPreTrigger : StatusEffectApplyXPreTrigger
    {
        private int removedTemp;

        private int removedCurrent;

        private int removedMax;

        private int effectAmountAdded;

        private bool cardPlayed;

        public StatusEffectTempConvertAttackToYPreTrigger()
        {
            scriptableAmount = ScriptableObject.CreateInstance<ScriptableFixedAmount>();
            ((ScriptableFixedAmount)scriptableAmount).amount = 0;
            eventPriority = -10000;
        }

        public override void Init()
        {
            base.PreTrigger += EntityPreTrigger;
            base.OnActionPerformed += ActionPerformed;
        }

        public new IEnumerator EntityPreTrigger(Trigger trigger)
        {
            if (oncePerTurn)
            {
                hasRunThisTurn = true;
            }

            running = true;
            ((ScriptableFixedAmount)scriptableAmount).amount = 0;
            int reduceTo = GetAmount();
            bool prompt = false;
            if (target.damage.current + target.tempDamage.Value > reduceTo)
            {
                // Current damage goes to reduceTo if higher than it
                // Temp damage needs to be set so damage+temp = reduceTo
                ((ScriptableFixedAmount)scriptableAmount).amount = target.damage.current + target.tempDamage.Value - reduceTo;
                if (target.damage.current > reduceTo)
                {
                    removedCurrent = target.damage.current - reduceTo;
                    target.damage.current = reduceTo;
                    prompt = true;
                }

                removedTemp = target.tempDamage.Value - (reduceTo - target.damage.current);
                target.tempDamage.Value = reduceTo - target.damage.current;
            }

            // Also reduce max if applicable
            if (target.damage.max > reduceTo)
            {
                removedMax = target.damage.max - reduceTo;
                target.damage.max = reduceTo;
                prompt = true;
            }
            if (prompt)
            {
                target.PromptUpdate();
            }
            if (((ScriptableFixedAmount)scriptableAmount).amount > 0)
            {
                effectAmountAdded += ((ScriptableFixedAmount)scriptableAmount).amount;
                yield return Run(runAgainst);
            }
            runAgainst = null;
            running = false;
        }

        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            if (!cardPlayed && entity == target)
            {
                cardPlayed = true;
            }

            return false;
        }

        public override bool RunActionPerformedEvent(PlayAction action)
        {
            if (cardPlayed)
            {
                return ActionQueue.Empty;
            }

            return false;
        }

        public IEnumerator ActionPerformed(PlayAction action)
        {
            cardPlayed = false;
            target.damage.current += removedCurrent;
            target.damage.max += removedMax;
            target.tempDamage.Value += removedTemp;
            if (removedCurrent != 0 || removedMax != 0)
            {
                target.PromptUpdate();
            }
            removedCurrent = 0;
            removedMax = 0;
            removedTemp = 0;
            if (effectAmountAdded > 0)
            {
                StatusEffectData addedEffect = target.statusEffects.Find((StatusEffectData a) => a.name.Equals(effectToApply.name));
                if (doPing)
                {
                    target.curveAnimator.Ping();
                }
                count = addedEffect.count - 1;
                yield return addedEffect.RemoveStacks(effectAmountAdded, removeTemporary: false);
            }
            effectAmountAdded = 0;
            yield break;
        }
    }

    public class StatusEffectEquipMask : StatusEffectData
    {
        public override void Init()
        {
            if (MainModFile.instance.maskedSpries.TryGetValue(target.data.name, out Sprite sprite))
            {
                target.data.mainSprite = sprite;
                target.gameObject.GetComponent<Card>().mainImage.sprite = sprite;
            }
        }
    }

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
        public StatusEffectInstantSummon summonCopy;
        public CardData.StatusEffectStacks[] addEffectStacks;
        public LocalizedString title;
        public bool addToDeck;

        private CardContainer _cardContainer;
        private GameObject _gameObject;
        private GameObject _objectGroup;

        private Entity _selected;
        private CardPocketSequence _sequence;

        public override IEnumerator Process()
        {
            _sequence = FindObjectOfType<CardPocketSequence>(true);
            var cc = (CardControllerSelectCard)_sequence.cardController;
            cc.pressEvent.AddListener(ChooseCard);
            cc.canPress = true;
            var container = GetCardContainer();

            if (source == CardSource.Custom)
                foreach (var entity in container)
                    yield return entity.GetCard().UpdateData();

            CinemaBarSystem.In();
            CinemaBarSystem.SetSortingLayer("UI2");
            if (!title.IsEmpty)
                CinemaBarSystem.Top.SetPrompt(title.GetLocalizedString(), "Select");
            _sequence.AddCards(container);
            yield return _sequence.Run();

            if (_selected != null) //Card Selected
            {
                Events.InvokeCardDraw(1);
                yield return Sequences.CardMove(_selected, new CardContainer[] { References.Player.handContainer });
                References.Player.handContainer.TweenChildPositions();
                Events.InvokeCardDrawEnd();
                _selected.flipper.FlipUp();
                yield return Sequences.WaitForAnimationEnd(_selected);
                yield return new ActionRunEnableEvent(_selected).Run();
                _selected.display.hover.enabled = true;

                foreach (var stack in addEffectStacks)
                    ActionQueue.Stack(new ActionApplyStatus(_selected, null, stack.data, stack.count));

                _selected.display.promptUpdateDescription = true;
                _selected.PromptUpdate();

                ActionQueue.Stack(new ActionSequence(_selected.UpdateTraits()) { note = $"[{_selected}] Update Traits" });

                _selected = null;
            }

            _cardContainer?.ClearAndDestroyAllImmediately();

            cc.canPress = false;
            cc.pressEvent.RemoveListener(ChooseCard);

            CinemaBarSystem.Clear();
            CinemaBarSystem.Out();

            yield return Remove();
        }

        private void ChooseCard(Entity entity)
        {
            _selected = entity;
            _sequence.promptEnd = true;

            if (!summonCopy)
                return;

            var cardData = _selected.data;
            summonCopy.targetSummon.summonCard = cardData;
            summonCopy.withEffects = new StatusEffectData[addEffectStacks.Length];
            for (int i = 0; i < addEffectStacks.Length; i++)
            {
                summonCopy.withEffects[i] = addEffectStacks[i].data;
            }
            ActionQueue.Stack(new ActionApplyStatus(target, target, summonCopy, copies));

            AddToDeck(cardData);

            _selected = null;
        }

        private void AddToDeck(CardData cardData)
        {
            if (!addToDeck)
                return;

            References.PlayerData.inventory.deck.Add(cardData);
            Events.InvokeEntityShowUnlocked(_selected);
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
                    _objectGroup = new GameObject("SelectCardRoutine");
                    _objectGroup.SetActive(false);
                    _objectGroup.transform.SetParent(GameObject.Find("Canvas/Padding/HUD/DeckpackLayout").transform.parent
                        .GetChild(0));
                    _objectGroup.transform.SetAsFirstSibling();

                    _gameObject = new GameObject("SelectCard");
                    var rect = _gameObject.AddComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(7, 2);

                    _cardContainer = CreateCardGrid(_objectGroup.transform, rect);

                    FillCardContainer(GetAmount());

                    _cardContainer.AssignController(Battle.instance.playerCardController);

                    return _cardContainer;
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

            amount = amount == 0 ? customCardList.Length : amount;
            foreach (var cardName in InPettyRandomOrder(customCardList).Take(amount))
            {
                var cardData = AddressableLoader.Get<CardData>("CardData", cardName).Clone();
                var card = CardManager.Get(cardData, Battle.instance.playerCardController, References.Player,
                    true,
                    true);
                _cardContainer.Add(card.entity);
            }
        }

        private void PredicateContainer(int amount)
        {
            Predicate<CardData> predicate = MainModFile.instance.predicateReferences[this.name];
            List<CardData> validCards = new List<CardData>();
            foreach (RewardPool pool in References.PlayerData.classData.rewardPools)
            {
                if (pool.type == "Items")
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

            for (int i = 0; i < amount; i++)
            {
                // Dont explode if we make too many choices
                if (validCards.Count() > 0)
                {
                    CardData randomCard = validCards.RandomItem();
                    validCards.Remove(randomCard);
                    _cardContainer.Add(CardManager.Get(randomCard.Clone(), Battle.instance.playerCardController, References.Player, inPlay: true, isPlayerCard: true).entity);
                }
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

        private static CardContainerGrid CreateCardGrid(Transform parent, Vector2 cellSize, int columnCount,
            RectTransform bounds = null)
        {
            var gridObj = new GameObject("CardGrid", typeof(RectTransform), typeof(CardContainerGrid));
            gridObj.transform.SetParent(bounds ?? parent);
            gridObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            var grid = gridObj.GetComponent<CardContainerGrid>();
            grid.holder = grid.GetComponent<RectTransform>();
            grid.onAdd = new UnityEventEntity(); // Fix null reference
            grid.onAdd.AddListener(entity =>
                entity.flipper.FlipUp()); // Flip up card when it's time (without waiting for others)
            grid.onRemove = new UnityEventEntity(); // Fix null reference

            grid.cellSize = cellSize;
            grid.columnCount = columnCount;

            AddScrollers(gridObj); // No click-and-drag. That needs Scroll View
            var scroller = gridObj.GetOrAdd<Scroller>();
            scroller.bounds = bounds; // Change scroller.bounds here if it only scrolls partially

            return grid;
        }

        private static void AddScrollers(GameObject parentObject)
        {
            var scroller = parentObject.GetOrAdd<Scroller>(); // Scroll with mouse
            parentObject.GetOrAdd<ScrollToNavigation>().scroller = scroller; // Scroll with controllers
            parentObject.GetOrAdd<TouchScroller>().scroller = scroller; // Scroll with touchscreen
        }
    }
}
