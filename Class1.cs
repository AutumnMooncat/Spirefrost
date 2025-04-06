using Deadpan.Enums.Engine.Components.Modding;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;
using TMPro;                  // Declares TMP_SpriteAsset
using WildfrostHopeMod.Utils; // Creates TMP_SpriteAsset
using WildfrostHopeMod.VFX;   // Declares StatusIconBuilder
using Extensions = Deadpan.Enums.Engine.Components.Modding.Extensions;
using System.Collections;


namespace SlayTheFrost
{
    public class MainModFile : WildfrostMod
    {
        public static MainModFile instance;
        public static List<object> assets = new List<object>();
        private bool preLoaded;

        // This allows for icons in descriptions
        public override TMP_SpriteAsset SpriteAsset => spriteAsset;
        internal static TMP_SpriteAsset spriteAsset;

        public MainModFile(string modDirectory) : base(modDirectory)
        { 
            instance = this;
        }

        public override string GUID => "autumnmooncat.wildfrost.spirefrost";
        public override string[] Depends => new string[] { "hope.wildfrost.vfx" };
        public override string Title => "Spirefrost";
        public override string Description => "Adds Slay the Spire content to Wildfrost.";

        private void CreateModAssets()
        {
            CreateTribes();

            CreateStatusEffects();

            CreateKeywords();

            CreateTraits();

            CreateIconBuilders();

            CreateLeaders();

            CreateCompanions();
            
            CreateSummons();

            CreateItems();

            CreateCharms();

            preLoaded = true;
        }

        private void CreateTribes()
        {
            // TRIBE
            assets.Add(TribeCopy("Basic", "Spire") //Snowdweller = "Basic", Shademancer = "Magic", Clunkmaster = "Clunk"
                .WithFlag("Images/SpireFlag.png")
                .WithSelectSfxEvent(FMODUnity.RuntimeManager.PathToEventReference("event:/sfx/status/shell")) //event:/sfx/status/heal
                .SubscribeToAfterAllBuildEvent(
                    (data) =>
                    {
                        GameObject gameObject = data.characterPrefab.gameObject.InstantiateKeepName();   //Copy the previous prefab
                        UnityEngine.Object.DontDestroyOnLoad(gameObject);                                //GameObject may be destroyed if their scene is unloaded. This ensures that will never happen to our gameObject
                        gameObject.name = "Player (Spirefrost.Spire)";                                   //For comparison, the snowdweller character is named "Player (Basic)"
                        data.characterPrefab = gameObject.GetComponent<Character>();                     //Set the characterPrefab to our new prefab
                        data.id = "Spirefrost.Spire";                                                    //Used to track win/loss statistics for the tribe. Not displayed anywhere though :/

                        data.leaders = DataList<CardData>("ironclad", "silent", "defect", "watcher");

                        Inventory inventory = ScriptableObject.CreateInstance<Inventory>();
                        inventory.deck.list = DataList<CardData>("kunai", "kunai", "kunai", "kunai", "shovel", "sundial", "medkit", "bronzeorb").ToList();
                        //Test Charms
                        //inventory.upgrades.Add(TryGet<CardUpgradeData>("CardUpgradeCritical"));
                        data.startingInventory = inventory;

                        RewardPool unitPool = CreateRewardPool("SpireUnitPool", "Units", DataList<CardData>(
                            "centurion", "mystic", "looter", "nob", "cultist", "fungi",
                            "jawworm", "slaver", "byrd", "chosen", "spikeslime", "fatgremlin",
                            "madgremlin", "shieldgremlin", "sneakygremlin","gremlinwizard"));

                        RewardPool itemPool = CreateRewardPool("SpireItemPool", "Items", DataList<CardData>(
                            "handdrill", "markofpain", "wristblade", "wingboots", "bluecandle",
                            "battery", "chemx", "pocketwatch", "fusionhammer", "lantern",
                            "icecream", "bandages", "anchor", "whetstone", "callingbell",
                            "gremlinhorn", "exploder", "repulser", "spiker", "orbwalker", 
                            "sphericguardian", "sentry", "bookofstabbing", "spirespear", "spireshield"));

                        //Dont forget Scrap Charm
                        RewardPool charmPool = CreateRewardPool("SpireCharmPool", "Charms", DataList<CardUpgradeData>(
                            "CultistPotionCharm", "FearPotionCharm", "StrengthPotionCharm",
                            "FairyPotionCharm", "WeakPotionCharm", "MiraclePotionCharm",
                            "IronPotionCharm", "CardUpgradeScrap"));

                        data.rewardPools = new RewardPool[]
                        {
                            unitPool,
                            itemPool,
                            charmPool,
                            Extensions.GetRewardPool("GeneralUnitPool"),
                            Extensions.GetRewardPool("GeneralItemPool"),
                            Extensions.GetRewardPool("GeneralCharmPool"),
                            Extensions.GetRewardPool("GeneralModifierPool"),
                            Extensions.GetRewardPool("SnowUnitPool"),         //
                            Extensions.GetRewardPool("SnowItemPool"),         //The snow pools are not Snowdwellers, there are general snow units/cards/charms.
                            Extensions.GetRewardPool("SnowCharmPool"),        //
                        };
                    })
            );
        }

        private void CreateStatusEffects()
        {
            // CONDITION EFFECTS
            assets.Add(new StatusEffectDataBuilder(this)
                .Create<StatusEffectSTSRegen>("STS Regen")
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectSTSRegen>(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintHasHealth>(),
                        ScriptableObject.CreateInstance<TargetConstraintIsAlive>()
                    };
                })
                .Subscribe_WithStatusIcon("STS Regen Icon")
            );
            
            assets.Add(new StatusEffectDataBuilder(this)
                .Create<StatusEffectSTSVulnerable>("STS Vuln")
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .WithOffensive(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectSTSVulnerable>(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintCanBeHit>()
                    };
                })
                .Subscribe_WithStatusIcon("STS Vuln Icon")
            );
            
            assets.Add(new StatusEffectDataBuilder(this)
                .Create<StatusEffectSTSWeakness>("STS Weak")
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .WithOffensive(true)
                //CanAttack constraint?
                .Subscribe_WithStatusIcon("STS Weak Icon")
            );
            
            assets.Add(new StatusEffectDataBuilder(this)
                .Create<StatusEffectSTSAmplify>("STS Amplify")
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectSTSAmplify>(data =>
                {
                    TargetConstraintMaxCounterMoreThan hasCounter = ScriptableObject.CreateInstance<TargetConstraintMaxCounterMoreThan>();
                    hasCounter.moreThan = 0;
                    TargetConstraintOr hasReactionOrCounter = ScriptableObject.CreateInstance<TargetConstraintOr>();
                    hasReactionOrCounter.constraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintHasReaction>(),
                        hasCounter
                        
                    };
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintCanBeBoosted>(),
                        ScriptableObject.CreateInstance<TargetConstraintIsUnit>(),
                        hasReactionOrCounter
                    };
                })
                .Subscribe_WithStatusIcon("STS Amplify Icon")
            );

            assets.Add(StatusCopy("MultiHit (Temporary, Not Visible)", "STS Double Tap")
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .Subscribe_WithStatusIcon("STS Double Tap Icon")
            );

            assets.Add(new StatusEffectDataBuilder(this)
                .Create<StatusEffectSTSFlight>("STS Flight")
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectSTSFlight>(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintCanBeHit>()
                    };
                })
                .Subscribe_WithStatusIcon("STS Flight Icon")
            );
            assets.Add(new StatusEffectDataBuilder(this)
                .Create<StatusEffectSTSMark>("STS Mark")
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectSTSMark>(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintCanBeHit>()
                    };
                })
                .Subscribe_WithStatusIcon("STS Mark Icon")
            );

            assets.Add(StatusCopy("When Redraw Hit Apply Attack & Health To Self", "STS Ritual")
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .WithIsKeyword(true)
                //.WithType("damage up")
                //.WithKeyword("<keyword=autumnmooncat.wildfrost.spirefrost.stsritual>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenRedrawHit>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Increase Attack");
                    data.applyConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintDoesDamage>()
                    };
                })
                .Subscribe_WithStatusIcon("STS Ritual Icon")
            );

            assets.Add(new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXWhenHitOnce>("STS Roll Up")
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenHitOnce>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Shell");
                    data.applyConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintCanBeHit>()
                    };
                })
                .Subscribe_WithStatusIcon("STS Roll Up Icon")
            );

            // TRAIT EFFECTS

            // GENERAL EFFECTS
            assets.Add(StatusCopy("When Card Destroyed, Gain Frenzy", "When Card Destroyed, Gain Shell")
                .WithText("When a card is destroyed, gain <+{a}><keyword=shell>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenCardDestroyed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Shell");
                })
            );

            assets.Add(StatusCopy("When Enemy Is Hit By Item Apply Demonize To Them", "When Enemy Is Hit By Item Apply Reduce Their Health")
                .WithText("When an enemy is hit with an <Item>, reduce their <keyword=health> by <{a}>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenUnitIsHit>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Reduce Max Health");
                })
            );

            assets.Add(StatusCopy("Summon Beepop", "Summon Lightning Orb")
                .WithText("Summon {0}")
                .WithTextInsert("<card=autumnmooncat.wildfrost.spirefrost.lightningorb>")
                .SubscribeToAfterAllBuildEvent<StatusEffectSummon>(data =>
                {
                    data.summonCard = TryGet<CardData>("lightningorb");
                })
            );

            assets.Add(StatusCopy("Summon Beepop", "Summon Dark Orb")
                .WithText("Summon {0}")
                .WithTextInsert("<card=autumnmooncat.wildfrost.spirefrost.darkorb>")
                .SubscribeToAfterAllBuildEvent<StatusEffectSummon>(data =>
                {
                    data.summonCard = TryGet<CardData>("darkorb");
                })
            );

            assets.Add(StatusCopy("Summon Beepop", "Summon Frost Orb")
                .WithText("Summon {0}")
                .WithTextInsert("<card=autumnmooncat.wildfrost.spirefrost.frostorb>")
                .SubscribeToAfterAllBuildEvent<StatusEffectSummon>(data =>
                {
                    data.summonCard = TryGet<CardData>("frostorb");
                })
            );

            assets.Add(StatusCopy("Summon Beepop", "Summon Plasma Orb")
                .WithText("Summon {0}")
                .WithTextInsert("<card=autumnmooncat.wildfrost.spirefrost.plasmaorb>")
                .SubscribeToAfterAllBuildEvent<StatusEffectSummon>(data =>
                {
                    data.summonCard = TryGet<CardData>("plasmaorb");
                })
            );

            assets.Add(StatusCopy("On Turn Apply Shell To Allies", "On Turn Apply Regen To Allies")
                .WithText("Apply <{a}><keyword=autumnmooncat.wildfrost.spirefrost.stsregen> to all allies")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnTurn>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("STS Regen");
                })
            );

            assets.Add(StatusCopy("On Kill Apply Gold To Self", "On Kill Trigger Again")
                .WithText("On kill, trigger again")
                .WithIsReaction(true)
                .WithCanBeBoosted(false)
                .WithStackable(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnKill>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
                    data.eventPriority = -99;
                    data.descColorHex = "F99C61";
                })

            );
            
            assets.Add(StatusCopy("Reduce Attack", "Reduce Attack With Text")
                .WithText("Reduce the attacker's <keyword=attack> by <{a}>")
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantReduceAttack>(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintDoesDamage>()
                    };
                })
            );

            assets.Add(new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXToFrontEnemiesWhenHit>("When Hit Apply Vuln To Front Enemies")
                .WithText("When hit, apply <{a}><keyword=autumnmooncat.wildfrost.spirefrost.stsvuln> to enemies in front")
                .WithCanBeBoosted(true)
                .WithOffensive(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXToFrontEnemiesWhenHit>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("STS Vuln");
                    data.applyConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintCanBeHit>()
                    };
                })
            );

            assets.Add(StatusCopy("When Healed Apply Attack To Self", "When Healed Reduce Counter")
                .WithText("When healed, count down <keyword=counter> by <{a}>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenHealed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Reduce Counter");
                })
            );

            assets.Add(new StatusEffectDataBuilder(this)
                .Create<StatusEffectInstantIncreaseCounter>("Increase Counter")
                .WithText("Count up <keyword=counter> by <{a}>")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantIncreaseCounter>(data =>
                {
                    TargetConstraintMaxCounterMoreThan hasCounter = ScriptableObject.CreateInstance<TargetConstraintMaxCounterMoreThan>();
                    hasCounter.moreThan = 0;
                    TargetConstraintHasStatus doesntHaveSnow = ScriptableObject.CreateInstance<TargetConstraintHasStatus>();
                    doesntHaveSnow.not = true;
                    doesntHaveSnow.status = TryGet<StatusEffectData>("Snow");
                    data.targetConstraints = new TargetConstraint[]
                    {
                        hasCounter,
                        doesntHaveSnow,
                        ScriptableObject.CreateInstance<TargetConstraintOnBoard>()
                    };
                })
            );

            assets.Add(StatusCopy("On Hit Pull Target", "On Hit Increase Counter")
                .WithText("Count up target's <keyword=counter> by <{a}>")
                .WithCanBeBoosted(true)
                .WithIsKeyword(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnHit>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Increase Counter");
                })
            );
            
            assets.Add(StatusCopy("When Deployed Apply Block To Self", "When Deployed Apply Flight To Self")
                .WithText("When deployed, gain <{a}><keyword=autumnmooncat.wildfrost.spirefrost.stsflight>")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenDeployed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("STS Flight");
                })
            );
            
            assets.Add(StatusCopy("While Active Increase Effects To FrontAlly", "While Active Increase Effects To AllyBehind")
                .WithText("While active, boost the effects of the ally behind by {a}")
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectWhileActiveX>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyBehind;
                })
            );
            
            assets.Add(StatusCopy("Split", "STS Split")
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantSplit>(data =>
                {
                    data.profiles = new StatusEffectInstantSplit.Profile[] {
                        new StatusEffectInstantSplit.Profile()
                        {
                            cardName = "autumnmooncat.wildfrost.spirefrost.spikeslime",
                            changeToCardName = "autumnmooncat.wildfrost.spirefrost.spikeslime2"
                        },
                        new StatusEffectInstantSplit.Profile()
                        {
                            cardName = "autumnmooncat.wildfrost.spirefrost.spikeslime2",
                            changeToCardName = "autumnmooncat.wildfrost.spirefrost.spikeslime3"
                        }
                    };
                })
            );
            
            assets.Add(StatusCopy("When X Health Lost Split", "When X Health Lost STS Split")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenHealthLost>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("STS Split");
                })
            );
            
            assets.Add(StatusCopy("On Card Played Boost To RandomEnemy", "On Card Played Increase Counter To RandomEnemy")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Increase Counter");
                })
            );

            assets.Add(StatusCopy("On Hit Equal Heal To FrontAlly", "On Hit Equal Shell To FrontAlly")
                .WithText("Apply <keyword=shell> to front ally equal to damage dealt")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnHit>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Shell");
                })
            );

            assets.Add(StatusCopy("On Card Played Add Zoomlin To Cards In Hand", "On Card Played Add Attack To Cards In Hand")
                .WithCanBeBoosted(true)
                .WithStackable(true)
                .WithText("Add <+{a}><keyword=attack> to all cards in your hand")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Increase Attack");
                })
            );

            assets.Add(StatusCopy("Reduce Attack", "Reduce Attack (With Text)")
                .WithText("Reduce <keyword=attack> by <{a}>")
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantReduceAttack>(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintDoesDamage>()
                    };
                })
            );

            assets.Add(StatusCopy("When Hit Reduce Attack To Attacker", "When Hit Reduce Effect To Attacker")
                .WithText("When hit, reduce the attacker's effects by <{a}>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenHit>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Reduce Effects");
                })
            );

            assets.Add(StatusCopy("While Active Frenzy To AlliesInRow", "While Active STS Ritual To AlliesInRow")
                .WithText("While active, add <x{a}><keyword=autumnmooncat.wildfrost.spirefrost.stsritual> to allies in the row")
                .SubscribeToAfterAllBuildEvent<StatusEffectWhileActiveX>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("STS Ritual");
                })
            );

            assets.Add(StatusCopy("When Ally Is Healed Apply Equal Spice", "When Ally Is Healed Apply Equal Shell")
                .WithText("When an ally is healed, apply equal <keyword=shell>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenAllyHealed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Shell");
                })
            );

            assets.Add(StatusCopy("When Redraw Hit Apply Attack & Health To Self", "When Redraw Hit Apply Shell To Self")
                .WithText("When <Redraw Bell> is hit, gain <{a}><keyword=shell>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenRedrawHit>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Shell");
                })
            );

            assets.Add(StatusCopy("When Card Destroyed, Gain Attack", "When Card Destroyed, Draw")
                .WithText("When a card is destroyed, draw <{a}>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenCardDestroyed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Instant Draw");
                })
            );

            assets.Add(StatusCopy("On Kill Increase Health To Self & Allies", "On Kill Increase Health To Self")
                .WithText("On kill, add <+{a}><keyword=health> to self")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnKill>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                })
            );

            assets.Add(StatusCopy("When Enemy Is Hit By Item Apply Demonize To Them", "When Enemy Is Hit By Item Apply Shroom To Them")
                .WithText("When an enemy is hit with an <Item>, apply <{a}><keyword=shroom> to them")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenUnitIsHit>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Shroom");
                })
            );

            assets.Add(StatusCopy("On Hit Damage Damaged Target", "On Hit Damage If Draw Pile Empty")
                .WithText("Deal <{a}> additional damage if your draw pile is empty")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnHit>(data =>
                {
                    TargetConstraintEmptyPile emptyDraw = ScriptableObject.CreateInstance<TargetConstraintEmptyPile>();
                    emptyDraw.pile = TargetConstraintEmptyPile.PileType.Draw;
                    data.applyConstraints = new TargetConstraint[]
                    {
                        emptyDraw
                    };
                })
            );

            assets.Add(new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXWhenAnyCardIsPlayed>("When Item Played, Increase Its Attack")
                .WithText("When an item is played, add <+{a}><keyword=attack> to it")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenAnyCardIsPlayed>(data =>
                {
                    data.targetPlayedCard = true;
                    data.effectToApply = TryGet<StatusEffectData>("Increase Attack");
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintIsItem>(),
                        ScriptableObject.CreateInstance<TargetConstraintDoesDamage>()
                    };
                })
            );

            assets.Add(new StatusEffectDataBuilder(this)
                .Create<StatusEffectInstantGainTrait>("Gain Explode")
                .WithText("Apply <{a}><keyword=explode>")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantGainTrait>(data =>
                {
                    data.traitToGain = TryGet<TraitData>("Explode");
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintIsUnit>(),
                        ScriptableObject.CreateInstance<TargetConstraintCanBeHit>()
                    };
                })
            );

            assets.Add(StatusCopy("On Turn Apply Teeth To Self", "On Turn Apply Explode To Self")
                .WithText("Gain <{a}><keyword=explode>")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnTurn>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Gain Explode");
                })
            );

            assets.Add(StatusCopy("On Card Played Reduce Counter To Allies", "On Card Played Reduce Counter To Random Ally")
                .WithText("Count down a random ally's <sprite name=counter> by <{a}>")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.RandomAlly;
                })
            );

            assets.Add(StatusCopy("On Card Played Add Gearhammer To Hand", "On Card Played Add Holy Water To Hand")
                .WithText("Add <{a}> {0} to your hand")
                .WithTextInsert("<card=autumnmooncat.wildfrost.spirefrost.holywater>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Instant Summon Holy Water In Hand");
                })
            );

            assets.Add(StatusCopy("Instant Summon Gearhammer In Hand", "Instant Summon Holy Water In Hand")
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantSummon>(data =>
                {
                    data.targetSummon = (StatusEffectSummon)TryGet<StatusEffectData>("Summon Holy Water");
                })
            );

            assets.Add(StatusCopy("Summon Gearhammer", "Summon Holy Water")
                .SubscribeToAfterAllBuildEvent<StatusEffectSummon>(data =>
                {
                    data.summonCard = TryGet<CardData>("holywater");
                })
            );

            assets.Add(StatusCopy("On Turn Heal & Cleanse Allies", "On Turn Cleanse Self")
                .WithText("<keyword=cleanse> self")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnTurn>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("Cleanse");
                })
            );

            assets.Add(new StatusEffectDataBuilder(this)
                .Create<StatusEffectApplyXWhenAnyCardIsPlayed>("When Attack Item Played, Reduce Counter")
                .WithText("When an <Item> with <keyword=attack> is played, count down <keyword=counter> by <{a}>")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenAnyCardIsPlayed>(data =>
                {
                    data.targetPlayedCard = false;
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("Reduce Counter");
                    data.triggerConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintIsItem>(),
                        ScriptableObject.CreateInstance<TargetConstraintDoesDamage>()
                    };
                })
            );
        }

        private void CreateKeywords()
        {
            // KEYWORDS
            // Keywords without pictures must use .WithShowName(true)
            assets.Add(new KeywordDataBuilder(this)
                .Create("stsregen")
                .WithTitle("Regen")
                .WithDescription("Heals health every turn | Counts down every turn")
                .WithTitleColour(new Color(0.5f, 1.0f, 0.5f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.49f, 0.99f, 0.49f))
                .WithCanStack(true)
            );
            
            assets.Add(new KeywordDataBuilder(this)
                .Create("stsvuln")
                .WithTitle("Vulnerable")
                .WithDescription("Increases damage taken by 50% for each Vulnerable | Clears after taking damage")
                .WithTitleColour(new Color(0.8f, 0.4f, 0.4f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.79f, 0.39f, 0.39f))
                .WithCanStack(true)
            );
            
            assets.Add(new KeywordDataBuilder(this)
                .Create("stsweak")
                .WithTitle("Weakened")
                .WithDescription("Halves damage dealt | Counts down after triggering")
                .WithTitleColour(new Color(0.7f, 1.0f, 0.75f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.69f, 0.99f, 0.74f))
                .WithCanStack(true)
            );
            
            assets.Add(new KeywordDataBuilder(this)
                .Create("stsamplify")
                .WithTitle("Amplify")
                .WithDescription("Increases target's effects | Clears after triggering")
                .WithTitleColour(new Color(0.25f, 0.75f, 0.85f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.24f, 0.74f, 0.84f))
                .WithCanStack(true)
            );
            
            assets.Add(new KeywordDataBuilder(this)
                .Create("stsdoubletap")
                .WithTitle("Double Tap")
                .WithDescription("Trigger additional times | Clears after triggering")
                .WithTitleColour(new Color(0.55f, 0.1f, 0.1f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.54f, 0.09f, 0.09f))
                .WithCanStack(true)
            ); 
            
            assets.Add(new KeywordDataBuilder(this)
                .Create("stsflight")
                .WithTitle("Flight")
                .WithDescription("Halves damage taken | Counts down after taking damage")
                .WithTitleColour(new Color(0.7f, 0.8f, 0.9f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.69f, 0.79f, 0.89f))
                .WithCanStack(true)
            );

            assets.Add(new KeywordDataBuilder(this)
                .Create("stsritual")
                .WithTitle("Ritual")
                .WithDescription("When <Redraw Bell> is hit, gain <keyword=attack>")
                .WithTitleColour(new Color(0.5f, 0.8f, 1.0f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.49f, 0.79f, 0.99f))
                .WithCanStack(true)
            );
            
            assets.Add(new KeywordDataBuilder(this)
                .Create("stsrollup")
                .WithTitle("Roll Up")
                .WithDescription("When hit, gain <keyword=shell> and lose Roll Up")
                .WithTitleColour(new Color(0.35f, 0.7f, 0.8f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.34f, 0.69f, 0.79f))
                .WithCanStack(true)
            );

            assets.Add(new KeywordDataBuilder(this)
                .Create("stsmark")
                .WithTitle("Mark")
                .WithDescription("When applied, all enemies take damage equal to their Mark | Does not count down!")
                .WithTitleColour(new Color(0.4f, 0.7f, 0.7f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.39f, 0.69f, 0.69f))
                .WithCanStack(true)
            );
        }

        private void CreateTraits()
        {
            // TRAITS
            /*assets.Add(new TraitDataBuilder(this)
                .Create("Ritual")
                .SubscribeToAfterAllBuildEvent(trait =>
                {
                    trait.keyword = TryGet<KeywordData>("stsritual");
                    trait.effects = new StatusEffectData[]
                    {
                        TryGet<StatusEffectData>("STS Ritual")
                    };
                })
            );*/
        }

        private void CreateIconBuilders()
        {
            // ICON BUILDERS
            // Default text color : 0.2471f, 0.1216f, 0.1647f, 1f
            assets.Add(new StatusIconBuilder(this)
                .Create("STS Regen Icon", "spirefrost.stsregen", ImagePath("Icons/RegenIcon.png"))
                .WithIconGroupName(StatusIconBuilder.IconGroups.health)
                .WithTextColour(new Color(0.2471f, 0.1216f, 0.1647f, 1f))
                .WithTextShadow(new Color(1.0f, 1.0f, 1.0f, 1.0f))
                .WithTextboxSprite()
                .WithKeywords("stsregen")
            );
            
            assets.Add(new StatusIconBuilder(this)
                .Create("STS Vuln Icon", "spirefrost.stsvuln", ImagePath("Icons/VulnIcon.png"))
                .WithIconGroupName(StatusIconBuilder.IconGroups.health)
                .WithTextColour(new Color(0.2471f, 0.1216f, 0.1647f, 1f))
                .WithTextShadow(new Color(1.0f, 1.0f, 1.0f, 1.0f))
                .WithTextboxSprite()
                .WithKeywords("stsvuln")
                .FreeModify(action =>
                {
                    action.textElement.outlineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    action.textElement.outlineWidth = 0.2f;
                    action.textElement.fontSharedMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.25f);
                })
            );
            
            assets.Add(new StatusIconBuilder(this)
                .Create("STS Weak Icon", "spirefrost.stsweak", ImagePath("Icons/WeakIcon.png"))
                .WithIconGroupName(StatusIconBuilder.IconGroups.damage)
                .WithTextColour(new Color(0.2471f, 0.1216f, 0.1647f, 1f))
                .WithTextShadow(new Color(1.0f, 1.0f, 1.0f, 1.0f))
                .WithTextboxSprite()
                .WithKeywords("stsweak")
                .FreeModify(action =>
                {
                    action.textElement.outlineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    action.textElement.outlineWidth = 0.2f;
                    action.textElement.fontSharedMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.25f);
                })
            );
            
            assets.Add(new StatusIconBuilder(this)
                .Create("STS Amplify Icon", "spirefrost.stsamplify", ImagePath("Icons/AmplifyIcon.png"))
                .WithIconGroupName(StatusIconBuilder.IconGroups.counter)
                .WithTextColour(new Color(0.2471f, 0.1216f, 0.1647f, 1f))
                .WithTextShadow(new Color(1.0f, 1.0f, 1.0f, 1.0f))
                .WithTextboxSprite()
                .WithKeywords("stsamplify")
                .FreeModify(action =>
                {
                    action.textElement.outlineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    action.textElement.outlineWidth = 0.2f;
                    action.textElement.fontSharedMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.25f);
                })
            );
            
            assets.Add(new StatusIconBuilder(this)
                .Create("STS Double Tap Icon", "spirefrost.stsdoubletap", ImagePath("Icons/DoubleTapIcon.png"))
                .WithIconGroupName(StatusIconBuilder.IconGroups.counter)
                .WithTextColour(new Color(0.2471f, 0.1216f, 0.1647f, 1f))
                .WithTextShadow(new Color(1.0f, 1.0f, 1.0f, 1.0f))
                .WithTextboxSprite()
                .WithKeywords("stsdoubletap")
                .FreeModify(action =>
                {
                    action.textElement.outlineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    action.textElement.outlineWidth = 0.2f;
                    action.textElement.fontSharedMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.25f);
                })
            );
            
            assets.Add(new StatusIconBuilder(this)
                .Create("STS Flight Icon", "spirefrost.stsflight", ImagePath("Icons/FlightIcon.png"))
                .WithIconGroupName(StatusIconBuilder.IconGroups.health)
                .WithTextColour(new Color(0.2471f, 0.1216f, 0.1647f, 1f))
                .WithTextShadow(new Color(1.0f, 1.0f, 1.0f, 1.0f))
                .WithTextboxSprite()
                .WithKeywords("stsflight")
            );

            assets.Add(new StatusIconBuilder(this)
                .Create("STS Ritual Icon", "spirefrost.stsritual", ImagePath("Icons/RitualIcon.png"))
                .WithIconGroupName(StatusIconBuilder.IconGroups.damage)
                .WithTextColour(new Color(0.2471f, 0.1216f, 0.1647f, 1f))
                .WithTextShadow(new Color(1.0f, 1.0f, 1.0f, 1.0f))
                .WithTextboxSprite()
                .WithKeywords("stsritual")
                .FreeModify(action =>
                {
                    action.textElement.outlineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    action.textElement.outlineWidth = 0.2f;
                    action.textElement.fontSharedMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.25f);
                })
            );

            assets.Add(new StatusIconBuilder(this)
                .Create("STS Roll Up Icon", "spirefrost.stsrollup", ImagePath("Icons/RollUpIcon.png"))
                .WithIconGroupName(StatusIconBuilder.IconGroups.health)
                .WithTextColour(new Color(0.2471f, 0.1216f, 0.1647f, 1f))
                .WithTextShadow(new Color(1.0f, 1.0f, 1.0f, 1.0f))
                .WithTextboxSprite()
                .WithKeywords("stsrollup")
                .FreeModify(action =>
                {
                    action.textElement.outlineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    action.textElement.outlineWidth = 0.2f;
                    action.textElement.fontSharedMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.25f);
                })
            );
            
            assets.Add(new StatusIconBuilder(this)
                .Create("STS Mark Icon", "spirefrost.stsmark", ImagePath("Icons/MarkIcon.png"))
                .WithIconGroupName(StatusIconBuilder.IconGroups.health)
                .WithTextColour(new Color(0.2471f, 0.1216f, 0.1647f, 1f))
                .WithTextShadow(new Color(1.0f, 1.0f, 1.0f, 1.0f))
                .WithTextboxSprite()
                .WithKeywords("stsmark")
                .FreeModify(action =>
                {
                    action.textElement.outlineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    action.textElement.outlineWidth = 0.2f;
                    action.textElement.fontSharedMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.25f);
                })
            );
        }

        private void CreateLeaders()
        {
            // LEADERS
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("ironclad", "Ironclad")
                .SetSprites("Leaders/Ironclad.png", "Leaders/IroncladBG.png")
                .SetStats(8, 3, 4)
                .WithValue(25)
                .WithCardType("Leader")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    CardScriptRunnable ironcladScript = ScriptableObject.CreateInstance<CardScriptRunnable>();
                    ironcladScript.runnable = card =>
                    {
                        card.GiveUpgrade();
                        int ability = new Vector2Int(0, 4).Random();
                        switch (ability)
                        {
                            // Feel No Pain
                            case 0:
                                card.SetRandomHealth(8, 12);
                                card.SetRandomDamage(4, 6);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomPassive("When Card Destroyed, Gain Shell", 1, 2);
                                break;

                            // Demon Form
                            case 1:
                                card.SetRandomHealth(6, 8);
                                card.SetRandomDamage(2, 3);
                                card.SetRandomCounter(4, 4);
                                card.SetRandomPassive("On Turn Apply Attack To Self", 1, 1);
                                break;

                            // Bash
                            case 2:
                                card.SetRandomHealth(8, 11);
                                card.SetRandomDamage(4, 6);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomActive("STS Vuln", 2, 3);
                                break;

                            // Dark Embrace
                            case 3:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(4, 5);
                                card.SetRandomCounter(4, 4);
                                card.SetRandomPassive("When Card Destroyed, Draw", 1, 1);
                                break;

                            // Feed
                            case 4:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(4, 6);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomPassive("On Kill Increase Health To Self", 2, 3);
                                break;
                        }
                    };

                    data.createScripts = new CardScript[]
                    {
                        ironcladScript
                    };
                })
            );

            assets.Add(new CardDataBuilder(this)
                .CreateUnit("silent", "Silent")
                .SetSprites("Leaders/Silent.png", "Leaders/SilentBG.png")
                .SetStats(6, 2, 3)
                .WithValue(25)
                .WithCardType("Leader")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    CardScriptRunnable silentScript = ScriptableObject.CreateInstance<CardScriptRunnable>();
                    silentScript.runnable = card =>
                    {
                        card.GiveUpgrade();
                        int ability = new Vector2Int(0, 4).Random();
                        switch (ability)
                        {
                            // Accuracy
                            case 0:
                                card.SetRandomHealth(7, 9);
                                card.SetRandomDamage(3, 4);
                                card.SetRandomCounter(4, 4);
                                card.SetRandomPassive("While Active Increase Attack To Items In Hand", 1, 1);
                                break;

                            // Envenom
                            case 1:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(2, 2);
                                card.SetRandomCounter(4, 4);
                                card.SetRandomPassive("When Enemy Is Hit By Item Apply Shroom To Them", 1, 1);
                                break;

                            // Grand Finale
                            case 2:
                                card.SetRandomHealth(8, 11);
                                card.SetRandomDamage(4, 6);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomPassive("On Hit Damage If Draw Pile Empty", 4, 4);
                                break;

                            // Malaise
                            case 3:
                                card.SetRandomHealth(6, 9);
                                card.SetRandomDamage(3, 5);
                                card.SetRandomCounter(5, 6);
                                card.SetRandomActive("STS Weak", 1, 1);
                                break;

                            // Caltrops
                            case 4:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(2, 3);
                                card.SetRandomCounter(3, 4);
                                card.SetRandomPassive("Teeth", 2, 3);
                                break;
                        }
                    };

                    data.createScripts = new CardScript[]
                    {
                        silentScript
                    };
                })
            );

            assets.Add(new CardDataBuilder(this)
                .CreateUnit("defect", "Defect")
                .SetSprites("Leaders/Defect.png", "Leaders/DefectBG.png")
                .SetStats(7, 3, 4)
                .WithValue(25)
                .WithCardType("Leader")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    CardScriptRunnable defectScript = ScriptableObject.CreateInstance<CardScriptRunnable>();
                    defectScript.runnable = card =>
                    {
                        card.GiveUpgrade();
                        int ability = new Vector2Int(0, 4).Random();
                        switch (ability)
                        {
                            // Lightning
                            case 0:
                                card.damage = 0;
                                card.hasAttack = false;
                                card.SetRandomHealth(7, 9);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomPassive("Summon Lightning Orb", 1, 1);
                                break;

                            // Dark
                            case 1:
                                card.damage = 0;
                                card.hasAttack = false;
                                card.SetRandomHealth(5, 7);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomPassive("Summon Dark Orb", 1, 1);
                                break;

                            // Plasma
                            case 2:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(5, 7);
                                card.SetRandomCounter(6, 7);
                                card.SetRandomPassive("Summon Plasma Orb", 1, 1);
                                break;

                            // Frost
                            case 3:
                                card.SetRandomHealth(6, 8);
                                card.SetRandomDamage(3, 5);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomPassive("Summon Frost Orb", 1, 1);
                                break;

                            // Claw
                            case 4:
                                card.SetRandomHealth(7, 9);
                                card.SetRandomDamage(3, 4);
                                card.SetRandomCounter(4, 4);
                                card.SetRandomPassive("When Item Played, Increase Its Attack", 1, 1);
                                break;
                        }
                    };

                    data.createScripts = new CardScript[]
                    {
                        defectScript
                    };
                })
            );

            assets.Add(new CardDataBuilder(this)
                .CreateUnit("watcher", "Watcher")
                .SetSprites("Leaders/Watcher.png", "Leaders/WatcherBG.png")
                .SetStats(7, 4, 5)
                .WithValue(25)
                .WithCardType("Leader")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    CardScriptRunnable watcherScript = ScriptableObject.CreateInstance<CardScriptRunnable>();
                    watcherScript.runnable = card =>
                    {
                        card.GiveUpgrade();
                        int ability = new Vector2Int(0, 4).Random();
                        switch (ability)
                        {
                            // Calm
                            case 0:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(4, 6);
                                card.SetRandomCounter(5, 5);
                                card.SetRandomPassive("On Card Played Add Zoomlin To Random Card In Hand", 1, 1);
                                break;

                            // Pressure Points
                            case 1:
                                card.damage = 0;
                                card.hasAttack = false;
                                card.SetRandomHealth(5, 7);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomActive("STS Mark", 3, 3);
                                break;

                            // Miracle
                            case 2:
                                card.SetRandomHealth(6, 8);
                                card.SetRandomDamage(4, 6);
                                card.SetRandomCounter(6, 7);
                                card.SetRandomPassive("On Card Played Add Holy Water To Hand", 1, 1);
                                break;

                            // Empty Mind
                            case 3:
                                card.traits = new List<CardData.TraitStacks> { TStack("Draw", 1) };
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(4, 5);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomPassive("On Turn Cleanse Self", 1, 1);
                                break;

                            // Follow Up
                            case 4:
                                card.SetRandomHealth(7, 9);
                                card.SetRandomDamage(3, 3);
                                card.SetRandomCounter(5, 6);
                                card.SetRandomPassive("When Attack Item Played, Reduce Counter", 1, 1);
                                break;
                        }
                    };

                    data.createScripts = new CardScript[]
                    {
                        watcherScript
                    };
                })
            );
        }

        private void CreateCompanions()
        {
            // PETS
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("louse", "Lousie")
                .SetSprites("Units/Louse.png","Units/LouseBG.png")
                .SetStats(3, 2, 4)
                .WithValue(25)
                .IsPet((ChallengeData)null, true)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Weak", 1)
                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Roll Up", 2)
                    };
                })
            );

            // UNITS
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("centurion", "Centurion")
                .SetSprites("Units/Centurion.png", "Units/CenturionBG.png")
                .SetStats(10, 3, 4)
                .WithValue(50)
                .SetTraits(TStack("Frontline", 1))
                .SetStartWithEffect(SStack("On Turn Apply Shell To Allies", 1))
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("mystic", "Mystic")
                .SetSprites("Units/Mystic.png", "Units/MysticBG.png")
                .SetStats(4, null, 4)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Turn Apply Regen To Allies", 2),
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("looter", "Looter")
                .SetSprites("Units/Looter.png", "Units/LooterBG.png")
                .SetStats(5, 4, 3)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Kill Apply Gold To Self", 5),
                        SStack("On Kill Trigger Again", 1)
                    };
                })
            );

            assets.Add(new CardDataBuilder(this)
                .CreateUnit("nob", "Nob")
                .SetSprites("Units/Nob.png", "Units/NobBG.png")
                .SetStats(12, 3, 5)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Vuln", 2)
                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("When Hit Increase Attack Effects To Self", 1)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("lagavulin", "Lagavulin")
                .SetSprites("Units/Lagavulin.png", "Units/LagavulinBG.png")
                .SetStats(8, 4, 0)
                .WithValue(50)
                .SetTraits(TStack("Smackback", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Shell", 4),
                        SStack("When Hit Reduce Attack To Attacker", 1)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("cultist", "Cultist")
                .SetSprites("Units/Cultist.png", "Units/CultistBG.png")
                .SetStats(6, 2, 5)
                .WithValue(50)
                .WithFlavour("Caw Caw!")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Ritual", 1)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("fungi", "Fungi Beast")
                .SetSprites("Units/Fungi.png", "Units/FungiBG.png")
                .SetStats(4, 1, 3)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("When Hit Apply Vuln To Front Enemies", 1)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("jawworm", "Jaw Worm")
                .SetSprites("Units/JawWorm.png", "Units/JawWormBG.png")
                .SetStats(4, 3, 3)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("When Healed Reduce Counter", 1)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("slaver", "Slaver")
                .SetSprites("Units/Slaver.png", "Units/SlaverBG.png")
                .SetStats(6, 3, 5)
                .WithValue(50)
                .SetTraits(TStack("Longshot", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Hit Increase Counter", 2)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("byrd", "Byrd")
                .SetSprites("Units/Byrd.png", "Units/ByrdBG.png")
                .SetStats(4, 1, 4)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("MultiHit", 2),
                        SStack("When Deployed Apply Flight To Self", 1)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("chosen", "Chosen")
                .SetSprites("Units/Chosen.png", "Units/ChosenBG.png")
                .SetStats(11, 1, 3)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("MultiHit", 1),
                        SStack("While Active Increase Effects To AllyBehind", 1)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("spikeslime", "Spike Slime")
                .SetSprites("Units/SpikeSlime.png", "Units/SpikeSlimeBG.png")
                .SetStats(6, 2, 3)
                .WithValue(50)
                .SetTraits(TStack("Barrage", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("When X Health Lost STS Split", 3)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("spikeslime2", "Spike Slime")
                .SetSprites("Units/SpikeSlime2.png", "Units/SpikeSlimeBG.png")
                .SetStats(6, 2, 3)
                .WithValue(50)
                .SetTraits(TStack("Barrage", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("When X Health Lost STS Split", 3)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("spikeslime3", "Spike Slime")
                .SetSprites("Units/SpikeSlime3.png", "Units/SpikeSlimeBG.png")
                .SetStats(6, 2, 3)
                .WithValue(50)
                .SetTraits(TStack("Barrage", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("When X Health Lost STS Split", 3)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("fatgremlin", "Fat Gremlin")
                .SetSprites("Units/FatGremlin.png", "Units/FatGremlinBG.png")
                .SetStats(3, 4, 0)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Weak", 1)
                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Trigger When Ally or Enemy Is Killed", 1)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("madgremlin", "Mad Gremlin")
                .SetSprites("Units/MadGremlin.png", "Units/MadGremlinBG.png")
                .SetStats(5, 2, 4)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("When Hit Gain Attack To Self (No Ping)", 1)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("shieldgremlin", "Shield Gremlin")
                .SetSprites("Units/ShieldGremlin.png", "Units/ShieldGremlinBG.png")
                .SetStats(3, null, 4)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Turn Apply Shell To AllyInFrontOf", 2)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("sneakygremlin", "Sneaky Gremlin")
                .SetSprites("Units/SneakyGremlin.png", "Units/SneakyGremlinBG.png")
                .SetStats(2, 2, 0)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Trigger Against When Ally Attacks", 1)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("gremlinwizard", "Gremlin Wizard")
                .SetSprites("Units/GremlinWizard.png", "Units/GremlinWizardBG.png")
                .SetStats(5, 6, 6)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("When Hit Reduce Counter To Self", 1)
                    };
                })
            );

            // CLUNKERS
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("bronzeorb", "Bronze Orb")
                .SetSprites("Units/BronzeOrb.png", "Units/BronzeOrbBG.png")
                .SetStats(null, null, 0)
                .WithCardType("Clunker")
                .WithValue(25)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Scrap", 1),
                        SStack("When Hit Apply Shell To AlliesInRow", 2)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("exploder", "Exploder")
                .SetSprites("Units/Exploder.png", "Units/Exploder.png")
                .SetStats(null, null, 3)
                .WithCardType("Clunker")
                .WithValue(50)
                .SetTraits(TStack("Explode", 8))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Scrap", 1),
                        SStack("Destroy Self After Turn", 1)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("repulser", "Repulser")
                .SetSprites("Units/Repulser.png", "Units/RepulserBG.png")
                .SetStats(null, null, 0)
                .WithCardType("Clunker")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Scrap", 1),
                        SStack("When Hit Reduce Effect To Attacker", 1)
                    };
                })
            );

            assets.Add(new CardDataBuilder(this)
                .CreateUnit("spiker", "Spiker")
                .SetSprites("Units/Spiker.png", "Units/SpikerBG.png")
                .SetStats(null, null, 3)
                .WithCardType("Clunker")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Scrap", 1),
                        SStack("Teeth", 2),
                        SStack("On Turn Apply Teeth To Self", 2)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("orbwalker", "Orb Walker")
                .SetSprites("Units/OrbWalker.png", "Units/OrbWalkerBG.png")
                .SetStats(null, null, 0)
                .WithCardType("Clunker")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Scrap", 1),
                        SStack("While Active STS Ritual To AlliesInRow", 1)
                    };
                })
            );

            assets.Add(new CardDataBuilder(this)
                .CreateUnit("sphericguardian", "Spheric Guardian")
                .SetSprites("Units/SphericGuardian.png", "Units/SphericGuardianBG.png")
                .SetStats(null, null, 0)
                .WithCardType("Clunker")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Scrap", 1),
                        SStack("Shell", 4),
                        SStack("When Ally Is Healed Apply Equal Shell", 1)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("sentry", "Sentry")
                .SetSprites("Units/Sentry.png", "Units/SentryBG.png")
                .SetStats(null, null, 0)
                .WithCardType("Clunker")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Scrap", 1),
                        SStack("While Active Reduce Attack To Enemies (No Ping, No Desc)", 2)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("bookofstabbing", "Book Of Stabbing")
                .SetSprites("Units/BookOfStabbing.png", "Units/BookOfStabbingBG.png")
                .SetStats(null, null, 0)
                .WithCardType("Clunker")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Scrap", 1),
                        SStack("While Active Frenzy To AlliesInRow", 1)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("spirespear", "Spire Spear")
                .SetSprites("Units/SpireSpear.png", "Units/SpireSpearBG.png")
                .SetStats(null, 2, 3)
                .WithCardType("Clunker")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Scrap", 3),
                        SStack("Pre Trigger Gain Temp MultiHit Equal To Scrap - 1", 1),
                        SStack("On Card Played Lose Scrap To Self", 1)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("spireshield", "Spire Shield")
                .SetSprites("Units/SpireShield.png", "Units/SpireShieldBG.png")
                .SetStats(null, 1, 0)
                .WithCardType("Clunker")
                .WithValue(50)
                .SetTraits(TStack("Smackback", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Scrap", 3),
                        SStack("Bonus Damage Equal To Scrap On Board", 1),
                        SStack("On Card Played Lose Scrap To Self", 1)
                    };
                })
            );
        }

        private void CreateSummons()
        {
            // SUMMONS
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("lightningorb", "Lightning")
                .SetSprites("Summons/LightningOrb.png", "Summons/LightningOrbBG.png")
                .SetStats(2, 3, 1)
                .WithCardType("Summoned")
                .SetTraits(TStack("Aimless", 1))
            );

            assets.Add(new CardDataBuilder(this)
                .CreateUnit("darkorb", "Dark")
                .SetSprites("Summons/DarkOrb.png", "Summons/DarkOrbBG.png")
                .SetStats(2, null, 1)
                .WithCardType("Summoned")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Turn Apply Explode To Self", 2)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("plasmaorb", "PlasmaOrb")
                .SetSprites("Summons/PlasmaOrb.png", "Summons/PlasmaOrbBG.png")
                .SetStats(2, null, 1)
                .WithCardType("Summoned")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Card Played Reduce Counter To Random Ally", 1)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateUnit("frostorb", "Frost")
                .SetSprites("Summons/FrostOrb.png", "Summons/FrostOrbBG.png")
                .SetStats(2, null, 1)
                .WithCardType("Summoned")
                .SetAttackEffect(SStack("Frost", 1))
            );
        }

        private void CreateItems()
        {
            // STARTERS
            assets.Add(new CardDataBuilder(this)
                .CreateItem("kunai", "Kunai")
                .SetSprites("Items/Kunai.png", "Items/KunaiBG.png")
                .WithValue(10)
                .SetDamage(2)
                .SetStartWithEffect(SStack("On Hit Damage Damaged Target", 1))
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateItem("shovel", "Shovel")
                .SetSprites("Items/Shovel.png", "Items/ShovelBG.png")
                .WithValue(30)
                .SetDamage(0)
                .SetAttackEffect(SStack("Snow", 2))
                .SetTraits(TStack("Draw", 1))
                .WithFlavour("Diggy diggy hole")
            );

            assets.Add(new CardDataBuilder(this)
                .CreateItem("sundial", "Sundial")
                .SetSprites("Items/Sundial.png", "Items/SundialBG.png")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Reduce Counter", 1), 
                        SStack("On Card Played Increase Counter To RandomEnemy", 1)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateItem("medkit", "Medical Kit")
                .SetSprites("Items/MedicalKit.png", "Items/MedicalKitBG.png")
                .WithValue(25)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Regen", 2)
                    };
                })
            );
            
            // DRAFTABLES
            assets.Add(new CardDataBuilder(this)
                .CreateItem("handdrill", "Hand Drill")
                .SetSprites("Items/HandDrill.png", "Items/HandDrillBG.png")
                .WithValue(45)
                .SetDamage(2)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Vuln", 2)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateItem("markofpain", "Mark Of Pain")
                .SetSprites("Items/MarkOfPain.png", "Items/MarkOfPainBG.png")
                .WithValue(50)
                .SetTraits(TStack("Barrage", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Vuln", 2)
                    };
                })
            );

            assets.Add(new CardDataBuilder(this)
                .CreateItem("wristblade", "Wrist Blade")
                .SetSprites("Items/WristBlade.png", "Items/WristBladeBG.png")
                .WithValue(55)
                .SetDamage(1)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Weak", 2)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateItem("wingboots", "Wing Boots")
                .SetSprites("Items/WingBoots.png", "Items/WingBootsBG.png")
                .WithValue(55)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Flight", 1)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateItem("bluecandle", "Blue Candle")
                .SetSprites("Items/BlueCandle.png", "Items/BlueCandleBG.png")
                .WithValue(60)
                .SetTraits(TStack("Consume", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Ritual", 1),
                        SStack("Reduce Max Health", 3)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateItem("battery", "Nuclear Battery")
                .SetSprites("Items/Battery.png", "Items/BatteryBG.png")
                .WithValue(50)
                .SetAttackEffect(SStack("Boost Effects", 1))
                .SetTraits(TStack("Consume", 1))
                .CanPlayOnHand(true)
            );

            assets.Add(new CardDataBuilder(this)
                .CreateItem("chemx", "Chemical X")
                .SetSprites("Items/ChemX.png", "Items/ChemXBG.png")
                .WithValue(50)
                .SetTraits(TStack("Consume", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Double Tap", 2)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateItem("pocketwatch", "Pocketwatch")
                .SetSprites("Items/Pocketwatch.png", "Items/PocketwatchBG.png")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Double Tap", 1),
                        SStack("Increase Counter", 1)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateItem("fusionhammer", "Fusion Hammer")
                .SetSprites("Items/FusionHammer.png", "Items/FusionHammerBG.png")
                .WithValue(50)
                .SetDamage(2)
                .SetTraits(TStack("Barrage", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Amplify", 1)
                    };
                })
            );

            assets.Add(new CardDataBuilder(this)
                .CreateItem("lantern", "Eerie Lantern")
                .SetSprites("Items/Lantern.png", "Items/LanternBG.png")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Amplify", 2),
                        SStack("Increase Counter", 1)
                    };
                })
            );

            assets.Add(new CardDataBuilder(this)
                .CreateItem("icecream", "Ice Cream")
                .SetSprites("Items/IceCream.png", "Items/IceCreamBG.png")
                .WithValue(50)
                .SetAttackEffect(SStack("Reduce Counter", 4), SStack("Snow", 2))
            );
            
            assets.Add(new CardDataBuilder(this)
                .CreateItem("bandages", "Tough Bandages")
                .SetSprites("Items/Bandages.png", "Items/BandagesBG.png")
                .WithValue(50)
                .SetAttackEffect(SStack("Increase Max Health", 1), SStack("Shell", 3))
            );

            assets.Add(new CardDataBuilder(this)
                .CreateItem("anchor", "Anchor")
                .SetSprites("Items/Anchor.png", "Items/AnchorBG.png")
                .WithValue(50)
                .SetDamage(3)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Hit Equal Shell To FrontAlly", 1)
                    };
                })
            );

            assets.Add(new CardDataBuilder(this)
                .CreateItem("whetstone", "Whetstone")
                .SetSprites("Items/Whetstone.png", "Items/WhetstoneBG.png")
                .WithValue(50)
                .CanPlayOnHand(true)
                .NeedsTarget(false)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Card Played Add Attack To Cards In Hand", 1)
                    };
                })
            );

            assets.Add(new CardDataBuilder(this)
                .CreateItem("callingbell", "Calling Bell")
                .SetSprites("Items/CallingBell.png", "Items/CallingBellBG.png")
                .WithValue(50)
                .CanPlayOnHand(true)
                .SetTraits(TStack("Consume", 1))
                .WithFlavour("Bing Bong")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Reduce Attack (With Text)", 2)
                    };
                })
            );

            assets.Add(new CardDataBuilder(this)
                .CreateItem("gremlinhorn", "Gremlin Horn")
                .SetSprites("Items/GremlinHorn.png", "Items/GremlinHornBG.png")
                .WithValue(50)
                .SetDamage(5)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Kill Draw", 2)
                    };
                })
            );

            // TOKENS
            assets.Add(new CardDataBuilder(this)
                .CreateItem("holywater", "Holy Water")
                .SetSprites("Items/HolyWater.png", "Items/HolyWaterBG.png")
                .WithValue(50)
                .SetAttackEffect(SStack("Reduce Counter", 3))
                .SetTraits(TStack("Consume", 1))
            );
        }

        private void CreateCharms()
        {
            // CHARMS
            TargetConstraintHasHealth hasHealth = ScriptableObject.CreateInstance<TargetConstraintHasHealth>();
            TargetConstraintCanBeHit canBeHit = ScriptableObject.CreateInstance<TargetConstraintCanBeHit>();
            /*assets.Add(new CardUpgradeDataBuilder(this)
                .Create("MooncatTestCharm")
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("TestCharm.png")
                .WithTitle("Mooncat Test Charm")
                .WithText($"Testing: Gain <3> <keyword=autumnmooncat.wildfrost.spirefrost.stsregen>")
                .WithTier(2) //Affects cost in shops
                .SetConstraints(hasHealth, canBeHit)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.effects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Regen", 3)
                    };
                })
            );*/

            TargetConstraintAttackMoreThan moreThan2Attack = ScriptableObject.CreateInstance<TargetConstraintAttackMoreThan>();
            moreThan2Attack.value = 2;
            TargetConstraintIsUnit isUnit = ScriptableObject.CreateInstance<TargetConstraintIsUnit>();
            assets.Add(new CardUpgradeDataBuilder(this)
                .Create("CultistPotionCharm")
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/CultistCharm.png")
                .WithTitle("Cultist Potion")
                .WithText($"Start with <1> <keyword=autumnmooncat.wildfrost.spirefrost.stsritual>\nReduce <keyword=attack> by <3>")
                .WithTier(2)
                .ChangeDamage(-3)
                .SetConstraints(moreThan2Attack, isUnit)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.effects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Ritual", 1)
                    };
                })
            );
            TargetConstraintPlayOnSlot playsOnBoard = ScriptableObject.CreateInstance<TargetConstraintPlayOnSlot>();
            playsOnBoard.board = true;
            TargetConstraintPlayOnSlot doesNotPlayOnSlot = ScriptableObject.CreateInstance<TargetConstraintPlayOnSlot>();
            doesNotPlayOnSlot.slot = true;
            doesNotPlayOnSlot.not = true;
            TargetConstraintMaxCounterMoreThan hasCounter = ScriptableObject.CreateInstance<TargetConstraintMaxCounterMoreThan>();
            hasCounter.moreThan = 0;
            TargetConstraintHasReaction hasReaction = ScriptableObject.CreateInstance<TargetConstraintHasReaction>();
            TargetConstraintIsItem isItem = ScriptableObject.CreateInstance<TargetConstraintIsItem>();
            TargetConstraintOr doesTrigger = ScriptableObject.CreateInstance<TargetConstraintOr>();
            doesTrigger.constraints = new TargetConstraint[]
            {
                hasCounter,
                hasReaction,
                isItem
            };
            assets.Add(new CardUpgradeDataBuilder(this)
                .Create("FearPotionCharm")
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/FearCharm.png")
                .WithTitle("Fear Potion")
                .WithText($"Apply <2> <keyword=autumnmooncat.wildfrost.spirefrost.stsvuln>")
                .WithTier(2)
                .SetConstraints(playsOnBoard, doesNotPlayOnSlot, doesTrigger)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Vuln", 2)
                    };
                })
            );

            TargetConstraintDoesDamage doesDamage = ScriptableObject.CreateInstance<TargetConstraintDoesDamage>();
            assets.Add(new CardUpgradeDataBuilder(this)
                .Create("StrengthPotionCharm")
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/StrengthCharm.png")
                .WithTitle("Strength Potion")
                .WithText($"Gain <+1><keyword=attack>")
                .WithTier(2)
                .SetConstraints(isUnit, doesDamage, doesTrigger)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.effects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Turn Apply Attack To Self", 1)
                    };
                })
            );
            
            assets.Add(new CardUpgradeDataBuilder(this)
                .Create("FairyPotionCharm")
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/FairyCharm.png")
                .WithTitle("Fairy in a Bottle")
                .WithText($"Start with <2> <keyword=autumnmooncat.wildfrost.spirefrost.stsflight>")
                .WithTier(1)
                .SetConstraints(canBeHit)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.effects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Flight", 2)
                    };
                })
            );

            assets.Add(new CardUpgradeDataBuilder(this)
                .Create("WeakPotionCharm")
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/WeakCharm.png")
                .WithTitle("Weak Potion")
                .WithText($"Apply <1> <keyword=autumnmooncat.wildfrost.spirefrost.stsweak>")
                .WithTier(2)
                .SetConstraints(playsOnBoard, doesNotPlayOnSlot, doesTrigger)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Weak", 1)
                    };
                })
            );

            assets.Add(new CardUpgradeDataBuilder(this)
                .Create("MiraclePotionCharm")
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/MiracleCharm.png")
                .WithTitle("Bottled Miracle")
                .WithText($"Count down all allies' <sprite name=counter> by <1>\nIncrease <keyword=counter> by <2>")
                .WithTier(3)
                .ChangeCounter(2)
                .SetConstraints(isUnit, hasCounter, hasReaction)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.effects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Card Played Reduce Counter To Allies", 1)
                    };
                })
            );

            TargetConstraintOr canAct = ScriptableObject.CreateInstance<TargetConstraintOr>();
            canAct.constraints = new TargetConstraint[] {
                hasCounter,
                hasReaction
            };
            assets.Add(new CardUpgradeDataBuilder(this)
                .Create("IronPotionCharm")
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/HeartOfIronCharm.png")
                .WithTitle("Heart of Iron")
                .WithText($"When <Redraw Bell> is hit, gain <1><keyword=shell>")
                .WithTier(1)
                .ChangeCounter(2)
                .SetConstraints(isUnit, canAct)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.effects = new CardData.StatusEffectStacks[]
                    {
                        SStack("When Redraw Hit Apply Shell To Self", 1)
                    };
                })
            );
        }

        public override void Load()
        {
            if (!preLoaded)
            { 
                // The spriteAsset has to be defined before any icons are made!
                spriteAsset = HopeUtils.CreateSpriteAsset(Title);
                CreateModAssets(); 
            }
            // Let our sprites automatically show up for icon descriptions
            SpriteAsset.RegisterSpriteAsset();
            base.Load();
            CreateLocalizedStrings();
            GameMode gameMode = TryGet<GameMode>("GameModeNormal"); //GameModeNormal is the standard game mode. 
            gameMode.classes = gameMode.classes.Append(TryGet<ClassData>("Spire")).ToArray();
            Events.OnEntityCreated += FixImage;
        }

        public override void Unload()
        {
            // Prevent our icons from accidentally showing up in descriptions when not loaded
            SpriteAsset.UnRegisterSpriteAsset();
            base.Unload();
            GameMode gameMode = TryGet<GameMode>("GameModeNormal");
            gameMode.classes = RemoveNulls(gameMode.classes); //Without this, a non-restarted game would crash on tribe selection
            UnloadFromClasses();
            Events.OnEntityCreated -= FixImage;
        }

        internal T TryGet<T>(string name) where T : DataFile
        {
            T data;
            if (typeof(StatusEffectData).IsAssignableFrom(typeof(T)))
                data = base.Get<StatusEffectData>(name) as T;
            else if (typeof(KeywordData).IsAssignableFrom(typeof(T)))
                data = base.Get<KeywordData>(name.ToLower()) as T;
            else
                data = base.Get<T>(name);

            if (data == null)
                throw new Exception($"TryGet Error: Could not find a [{typeof(T).Name}] with the name [{name}] or [{Extensions.PrefixGUID(name, this)}]");

            return data;
        }

        private CardData.StatusEffectStacks SStack(string name, int amount) => new CardData.StatusEffectStacks(TryGet<StatusEffectData>(name), amount);

        private CardData.TraitStacks TStack(string name, int amount) => new CardData.TraitStacks(TryGet<TraitData>(name), amount);

        //Helper Method
        private RewardPool CreateRewardPool(string name, string type, DataFile[] list)
        {
            RewardPool pool = ScriptableObject.CreateInstance<RewardPool>();
            pool.name = name;
            pool.type = type;            //The usual types are Units, Items, Charms, and Modifiers.
            pool.list = list.ToList();
            return pool;
        }

        //Note: you need to add the reference DeadExtensions.dll in order to use InstantiateKeepName(). 
        public StatusEffectDataBuilder StatusCopy(string oldName, string newName)
        {
            StatusEffectData data = TryGet<StatusEffectData>(oldName).InstantiateKeepName();
            data.name = GUID + "." + newName;
            data.targetConstraints = new TargetConstraint[0];
            StatusEffectDataBuilder builder = data.Edit<StatusEffectData, StatusEffectDataBuilder>();
            builder.Mod = this;
            return builder;
        }

        private CardDataBuilder CardCopy(string oldName, string newName) => DataCopy<CardData, CardDataBuilder>(oldName, newName);

        private ClassDataBuilder TribeCopy(string oldName, string newName) => DataCopy<ClassData, ClassDataBuilder>(oldName, newName);

        private T DataCopy<Y, T>(string oldName, string newName) where Y : DataFile where T : DataFileBuilder<Y, T>, new()
        {
            Y data = Get<Y>(oldName).InstantiateKeepName();
            data.name = GUID + "." + newName;
            T builder = data.Edit<Y, T>();
            builder.Mod = this;
            return builder;
        }

        private T[] DataList<T>(params string[] names) where T : DataFile => names.Select((s) => TryGet<T>(s)).ToArray();

        //Credits to Hopeful for this method
        public override List<T> AddAssets<T, Y>()
        {
            if (assets.OfType<T>().Any())
                Debug.LogWarning($"[{Title}] adding {typeof(Y).Name}s: {assets.OfType<T>().Select(a => a._data.name).Join()}");
            return assets.OfType<T>().ToList();
        }

        public void UnloadFromClasses()
        {
            List<ClassData> tribes = AddressableLoader.GetGroup<ClassData>("ClassData");
            foreach (ClassData tribe in tribes)
            {
                if (tribe == null || tribe.rewardPools == null) { continue; } //This isn't even a tribe; skip it.

                foreach (RewardPool pool in tribe.rewardPools)
                {
                    if (pool == null) { continue; }
                    //This isn't even a reward pool; skip it.

                    pool.list.RemoveAllWhere((item) => item == null || item.ModAdded == this); //Find and remove everything that needs to be removed.
                }
            }
        }

        internal T[] RemoveNulls<T>(T[] data) where T : DataFile
        {
            List<T> list = data.ToList();
            list.RemoveAll(x => x == null || x.ModAdded == this);
            return list.ToArray();
        }

        //Remember to hook this method onto Events.OnEntityCreated in the Load/Unload (see Tutorial 1 or the full code for more details).
        private void FixImage(Entity entity)
        {
            if (entity.display is Card card && !card.hasScriptableImage) //These cards should use the static image
            {
                card.mainImage.gameObject.SetActive(true);               //And this line turns them on
            }
        }

        public string TribeTitleKey => GUID + ".TribeTitle";
        public string TribeDescKey => GUID + ".TribeDesc";

        //Call this method in Load()
        private void CreateLocalizedStrings()
        {
            StringTable uiText = LocalizationHelper.GetCollection("UI Text", SystemLanguage.English);
            uiText.SetString(TribeTitleKey, "The Ascenders");                                       //Create the title
            uiText.SetString(TribeDescKey, "Denizens of the spire have formed an unlikely team after finding themselves in an unknown place. " +
                "\n\n" +
                "Well versed in defending themselves, they take their enemies down with temporary effects to win the war of attrition.");                                  //Create the description.

        }
    }

    public class StatusEffectSTSRegen : StatusEffectData
    {
        public bool subbed;

        public bool primed;

        public bool doPing = true;

        public override void Init()
        {
            base.OnTurnEnd += HealTarget;
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
            if (primed && target.enabled)
            {
                return entity == target;
            }

            return false;
        }

        public IEnumerator HealTarget(Entity entity)
        {
            if (doPing)
            {
                target.curveAnimator?.Ping();
            }
            Hit hit = new Hit(GetDamager(), target, -count);
            yield return hit.Process();
            yield return Sequences.Wait(0.2f);
            int amount = 1;
            Events.InvokeStatusEffectCountDown(this, ref amount);
            if (amount != 0)
            {
                yield return CountDown(entity, amount);
            }
        }
    }

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
        public int toClear;

        public int amountRemoved;

        public StatusEffectSTSWeakness()
        {
            eventPriority = -999999;
            removeOnDiscard = true;
        }

        public override void Init()
        {
            base.OnActionPerformed += ActionPerformed;
        }

        public override bool RunCardPlayedEvent(Entity entity, Entity[] targets)
        {
            if (toClear == 0 && entity == target && count > 0 && targets != null && targets.Length > 0)
            {
                toClear = 1;
                amountRemoved = Mathf.CeilToInt(target.tempDamage.Value / 2f);
                target.tempDamage -= amountRemoved;
            }

            return false;
        }

        public override bool RunActionPerformedEvent(PlayAction action)
        {
            if (toClear > 0)
            {
                return ActionQueue.Empty;
            }

            return false;
        }

        public IEnumerator ActionPerformed(PlayAction action)
        {
            yield return Clear(toClear);
            toClear = 0;
            target.tempDamage += amountRemoved;
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
            if (!cardPlayed && entity == target && count > 0 && targets != null && targets.Length != 0)
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
            foreach (Entity entity in Battle.GetAllUnits(Battle.GetOpponent(target.owner)))
            {
                foreach (StatusEffectData effect in entity.statusEffects)
                {
                    if (effect is StatusEffectSTSMark && effect.count > 0)
                    {
                        // Hit em
                        Hit hit = new Hit(GetDamager(), entity, effect.count)
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
    }

    public class StatusEffectApplyXWhenHitOnce : StatusEffectApplyXWhenHit
    {
        public override void Init()
        {
            base.Init();
            base.PostHit += RemoveMe;
        }

        public IEnumerator RemoveMe(Hit hit)
        {
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
        [SerializeField]
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
            return Run(GetTargets(null, GetWasInRows(entity, targets), null, targets));
        }
    }

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
                    return !not;
                }
                return not;
            }

            if (pile == PileType.Hand)
            {
                CardContainer handContainer = References.Player.handContainer;
                if (handContainer != null && handContainer.Count > 0)
                {
                    return !not;
                }
                return not;
            }

            if (pile == PileType.Discard)
            {
                CardContainer discardContainer = References.Player.discardContainer;
                if (discardContainer != null && discardContainer.Count > 0)
                {
                    return !not;
                }
                return not;
            }

            return false;
        }

        public override bool Check(CardData targetData)
        {
            return Check((Entity)null);
        }
    }

    public class CardScriptRunnable : CardScript
    {
        public delegate void ToRun(CardData data);

        public ToRun runnable;
        public override void Run(CardData target)
        {
            runnable(target);
        }
    }

    internal static class CardDataExtensions
    {
        internal static void GiveUpgrade(this CardData target, string name = "Crown") // Give a crown by default
        {
            MainModFile.instance.TryGet<CardUpgradeData>(name).Clone().Assign(target);
        }

        internal static void SetRandomHealth(this CardData target, int min, int max)
        {
            if (target.hasHealth)
            {
                target.hp = new Vector2Int(min, max).Random();
                target.hp = Mathf.Max(1, target.hp);
            }
        }

        internal static void SetRandomDamage(this CardData target, int min, int max)
        {
            if (target.hasAttack)
            {
                target.damage = new Vector2Int(min, max).Random();
                target.damage = Mathf.Max(0, target.damage);
            }
        }

        internal static void SetRandomCounter(this CardData target, int min, int max)
        {
            if (target.counter >= 1)
            {
                target.counter = new Vector2Int(min, max).Random();
                target.counter = Mathf.Max(1, target.counter);
            }
        }

        internal static void SetRandomPassive(this CardData target, string passiveEffect, int min, int max)
        {
            StatusEffectData effect = MainModFile.instance.TryGet<StatusEffectData>(passiveEffect);
            target.startWithEffects = target.startWithEffects.With(new CardData.StatusEffectStacks(effect, new Vector2Int(min, max).Random()));
        }

        internal static void SetRandomActive(this CardData target, string passiveEffect, int min, int max)
        {
            StatusEffectData effect = MainModFile.instance.TryGet<StatusEffectData>(passiveEffect);
            target.attackEffects = target.attackEffects.With(new CardData.StatusEffectStacks(effect, new Vector2Int(min, max).Random()));
        }
    }

    [HarmonyPatch(typeof(References), nameof(References.Classes), MethodType.Getter)]
    static class FixClassesGetter
    {
        static void Postfix(ref ClassData[] __result) => __result = AddressableLoader.GetGroup<ClassData>("ClassData").ToArray();
    }

    [HarmonyPatch(typeof(TribeHutSequence), "SetupFlags")]
    class PatchTribeHut
    {
        static string TribeName = "Spire";
        static void Postfix(TribeHutSequence __instance)
        {
            GameObject gameObject = GameObject.Instantiate(__instance.flags[0].gameObject);
            gameObject.transform.SetParent(__instance.flags[0].gameObject.transform.parent, false);
            TribeFlagDisplay flagDisplay = gameObject.GetComponent<TribeFlagDisplay>();
            ClassData tribe = MainModFile.instance.TryGet<ClassData>(TribeName);
            flagDisplay.flagSprite = tribe.flag;
            __instance.flags = __instance.flags.Append(flagDisplay).ToArray();
            flagDisplay.SetAvailable();
            flagDisplay.SetUnlocked();

            TribeDisplaySequence sequence2 = GameObject.FindObjectOfType<TribeDisplaySequence>(true);
            GameObject gameObject2 = GameObject.Instantiate(sequence2.displays[1].gameObject);
            gameObject2.transform.SetParent(sequence2.displays[2].gameObject.transform.parent, false);
            sequence2.tribeNames = sequence2.tribeNames.Append(TribeName).ToArray();
            sequence2.displays = sequence2.displays.Append(gameObject2).ToArray();

            Button button = flagDisplay.GetComponentInChildren<Button>();
            button.onClick.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.Off);
            button.onClick.AddListener(() => { sequence2.Run(TribeName); });

            //(SfxOneShot)
            gameObject2.GetComponent<SfxOneshot>().eventRef = FMODUnity.RuntimeManager.PathToEventReference("event:/sfx/status/shell");

            //0: Flag (ImageSprite)
            gameObject2.transform.GetChild(0).GetComponent<ImageSprite>().SetSprite(tribe.flag);

            //1: Left (ImageSprite)
            Sprite needle = MainModFile.instance.TryGet<CardData>("ironclad").mainSprite;
            gameObject2.transform.GetChild(1).GetComponent<ImageSprite>().SetSprite(needle);

            //2: Right (ImageSprite)
            Sprite muncher = MainModFile.instance.TryGet<CardData>("silent").mainSprite;
            gameObject2.transform.GetChild(2).GetComponent<ImageSprite>().SetSprite(muncher);
            gameObject2.transform.GetChild(2).localScale *= 1.2f;

            //3: Textbox (Image)
            gameObject2.transform.GetChild(3).GetComponent<Image>().color = new Color(0.12f, 0.47f, 0.57f);

            //3-0: Text (LocalizedString)
            StringTable collection = LocalizationHelper.GetCollection("UI Text", SystemLanguage.English);
            gameObject2.transform.GetChild(3).GetChild(0).GetComponent<LocalizeStringEvent>().StringReference = collection.GetString(MainModFile.instance.TribeDescKey);

            //4:Title Ribbon (Image)
            //4-0: Text (LocalizedString)
            gameObject2.transform.GetChild(4).GetChild(0).GetComponent<LocalizeStringEvent>().StringReference = collection.GetString(MainModFile.instance.TribeTitleKey);
        }
    }
}
