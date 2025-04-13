using Deadpan.Enums.Engine.Components.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using WildfrostHopeMod.VFX;

namespace Spirefrost
{
    internal class SpirefrostAssetHandler
    {
        public static List<object> assets = new List<object>();

        private static T TryGet<T>(string name) where T : DataFile
        {
            return MainModFile.instance.TryGet<T>(name);
        }

        private static CardData.StatusEffectStacks SStack(string name, int amount) => new CardData.StatusEffectStacks(TryGet<StatusEffectData>(name), amount);

        private static CardData.TraitStacks TStack(string name, int amount) => new CardData.TraitStacks(TryGet<TraitData>(name), amount);

        //Helper Method
        private static RewardPool CreateRewardPool(string name, string type, DataFile[] list)
        {
            RewardPool pool = ScriptableObject.CreateInstance<RewardPool>();
            pool.name = name;
            pool.type = type;            //The usual types are Units, Items, Charms, and Modifiers.
            pool.list = list.ToList();
            return pool;
        }

        //Note: you need to add the reference DeadExtensions.dll in order to use InstantiateKeepName(). 
        private static StatusEffectDataBuilder StatusCopy(string oldName, string newName)
        {
            StatusEffectData data = TryGet<StatusEffectData>(oldName).InstantiateKeepName();
            data.name = MainModFile.instance.GUID + "." + newName;
            data.targetConstraints = new TargetConstraint[0];
            StatusEffectDataBuilder builder = data.Edit<StatusEffectData, StatusEffectDataBuilder>();
            builder.Mod = MainModFile.instance;
            return builder;
        }

        private static CardDataBuilder CardCopy(string oldName, string newName) => DataCopy<CardData, CardDataBuilder>(oldName, newName);

        private static ClassDataBuilder TribeCopy(string oldName, string newName) => DataCopy<ClassData, ClassDataBuilder>(oldName, newName);

        private static T DataCopy<Y, T>(string oldName, string newName) where Y : DataFile where T : DataFileBuilder<Y, T>, new()
        {
            Y data = MainModFile.instance.Get<Y>(oldName).InstantiateKeepName();
            data.name = MainModFile.instance.GUID + "." + newName;
            T builder = data.Edit<Y, T>();
            builder.Mod = MainModFile.instance;
            return builder;
        }

        private static T[] DataList<T>(params string[] names) where T : DataFile => names.Select((s) => TryGet<T>(s)).ToArray();

        internal static void CreateAssets()
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
        }

        private static void CreateTribes()
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
                        inventory.deck.list = DataList<CardData>("shuriken", "shuriken", "shuriken", "shuriken", "shovel", "sundial", "medkit", "bronzeorb").ToList();
                        //Test Charms
                        //inventory.upgrades.Add(TryGet<CardUpgradeData>("CardUpgradeCritical"));
                        data.startingInventory = inventory;

                        RewardPool unitPool = CreateRewardPool("SpireUnitPool", "Units", DataList<CardData>(
                            "centurion", "mystic", "looter", "nob", "cultist", "fungi",
                            "jawworm", "slaver", "byrd", "chosen", "spikeslime", "fatgremlin",
                            "madgremlin", "shieldgremlin", "sneakygremlin", "gremlinwizard"));

                        RewardPool itemPool = CreateRewardPool("SpireItemPool", "Items", DataList<CardData>(
                            "handdrill", "markofpain", "wristblade", "wingboots", "battery", 
                            "chemx", "pocketwatch", "fusionhammer", "lantern", "icecream", 
                            "bandages", "anchor", "whetstone", "callingbell", "gremlinhorn", 
                            "toolbox", "boot", "cultistmask", "kunai",
                            "exploder", "repulser", "spiker", "orbwalker", "sphericguardian", 
                            "sentry", "bookofstabbing", "spirespear", "spireshield"));

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

        private static void CreateStatusEffects()
        {
            // CONDITION EFFECTS
            assets.Add(new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXAfterTurnAndDecay>("STS Regen")
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXAfterTurnAndDecay>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Heal");
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintHasHealth>()
                    };
                })
                .Subscribe_WithStatusIcon("STS Regen Icon")
            );

            assets.Add(new StatusEffectDataBuilder(MainModFile.instance)
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

            assets.Add(new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectSTSWeakness>("STS Weak")
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .WithOffensive(true)
                //CanAttack constraint?
                .Subscribe_WithStatusIcon("STS Weak Icon")
            );

            assets.Add(new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectSTSAmplify>("STS Amplify")
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectSTSAmplify>(data =>
                {
                    TargetConstraintMaxCounterMoreThan hasCounter = ScriptableObject.CreateInstance<TargetConstraintMaxCounterMoreThan>();
                    hasCounter.moreThan = 0;
                    TargetConstraintOr doesTrigger = ScriptableObject.CreateInstance<TargetConstraintOr>();
                    doesTrigger.constraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintIsItem>(),
                        ScriptableObject.CreateInstance<TargetConstraintHasReaction>(),
                        hasCounter
                    };
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintCanBeBoosted>(),
                        doesTrigger
                    };
                })
                .Subscribe_WithStatusIcon("STS Amplify Icon")
            );

            assets.Add(new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectDoubleTap>("STS Double Tap")
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectDoubleTap>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("MultiHit");
                })
                .Subscribe_WithStatusIcon("STS Double Tap Icon")
            );

            assets.Add(new StatusEffectDataBuilder(MainModFile.instance)
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
            assets.Add(new StatusEffectDataBuilder(MainModFile.instance)
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
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintDoesDamage>()
                    };
                })
                .Subscribe_WithStatusIcon("STS Ritual Icon")
            );

            assets.Add(new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXWhenHitOnce>("STS Roll Up")
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenHitOnce>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("Shell");
                    data.targetConstraints = new TargetConstraint[]
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

            assets.Add(StatusCopy("On Turn Summon Bootleg Copy of RandomEnemy", "On Turn Summon Lightning Orb")
                .WithText("Summon {0}")
                .WithTextInsert("<card=autumnmooncat.wildfrost.spirefrost.lightningorb>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnTurn>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("Instant Summon Lightning Orb At Appliers Position");
                })
            );

            assets.Add(StatusCopy("Instant Summon Bootleg Copy At Appliers Position", "Instant Summon Lightning Orb At Appliers Position")
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantSummon>(data =>
                {
                    data.withEffects = new StatusEffectData[0];
                    data.summonCopy = false;
                    data.targetSummon = (StatusEffectSummon)TryGet<StatusEffectData>("Summon Lightning Orb");
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

            assets.Add(StatusCopy("On Turn Summon Bootleg Copy of RandomEnemy", "On Turn Summon Dark Orb")
                .WithText("Summon {0}")
                .WithTextInsert("<card=autumnmooncat.wildfrost.spirefrost.darkorb>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnTurn>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("Instant Summon Dark Orb At Appliers Position");
                })
            );

            assets.Add(StatusCopy("Instant Summon Bootleg Copy At Appliers Position", "Instant Summon Dark Orb At Appliers Position")
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantSummon>(data =>
                {
                    data.withEffects = new StatusEffectData[0];
                    data.summonCopy = false;
                    data.targetSummon = (StatusEffectSummon)TryGet<StatusEffectData>("Summon Dark Orb");
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

            assets.Add(StatusCopy("On Turn Summon Bootleg Copy of RandomEnemy", "On Turn Summon Frost Orb")
                .WithText("Summon {0}")
                .WithTextInsert("<card=autumnmooncat.wildfrost.spirefrost.frostorb>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnTurn>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("Instant Summon Frost Orb At Appliers Position");
                })
            );

            assets.Add(StatusCopy("Instant Summon Bootleg Copy At Appliers Position", "Instant Summon Frost Orb At Appliers Position")
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantSummon>(data =>
                {
                    data.withEffects = new StatusEffectData[0];
                    data.summonCopy = false;
                    data.targetSummon = (StatusEffectSummon)TryGet<StatusEffectData>("Summon Frost Orb");
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

            assets.Add(StatusCopy("On Turn Summon Bootleg Copy of RandomEnemy", "On Turn Summon Plasma Orb")
                .WithText("Summon {0}")
                .WithTextInsert("<card=autumnmooncat.wildfrost.spirefrost.plasmaorb>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnTurn>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("Instant Summon Plasma Orb At Appliers Position");
                })
            );

            assets.Add(StatusCopy("Instant Summon Bootleg Copy At Appliers Position", "Instant Summon Plasma Orb At Appliers Position")
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantSummon>(data =>
                {
                    data.withEffects = new StatusEffectData[0];
                    data.summonCopy = false;
                    data.targetSummon = (StatusEffectSummon)TryGet<StatusEffectData>("Summon Plasma Orb");
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

            assets.Add(new StatusEffectDataBuilder(MainModFile.instance)
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

            assets.Add(new StatusEffectDataBuilder(MainModFile.instance)
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

            assets.Add(StatusCopy("On Turn Apply Shell To Allies", "On Turn Apply Amplify AllyBehind")
                .WithText("Apply <{a}><keyword=autumnmooncat.wildfrost.spirefrost.stsamplify> ally behind")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnTurn>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("STS Amplify");
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
                .WithText("Count up <keyword=counter> of a random enemy by <{a}>")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Increase Counter");
                    data.noTargetType = NoTargetType.NoTargetForStatus;
                    data.noTargetTypeArgs = new string[] { "<sprite name=counter>" };
                    data.type = "counter down";
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
                .WithText("While active, add <{a}><keyword=autumnmooncat.wildfrost.spirefrost.stsritual> to allies in the row")
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

            assets.Add(new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXWhenAnyCardIsPlayed>("When Item Played, Increase Its Attack")
                .WithText("When an item is played, add <+{a}><keyword=attack> to it")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenAnyCardIsPlayed>(data =>
                {
                    data.targetPlayedCard = true;
                    data.effectToApply = TryGet<StatusEffectData>("Increase Attack");
                    data.applyConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintIsItem>(),
                        ScriptableObject.CreateInstance<TargetConstraintDoesDamage>()
                    };
                })
            );

            assets.Add(new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectInstantGainTrait>("Gain Explode")
                .WithText("Apply <{a}> <keyword=explode>")
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
                .WithText("Gain <{a}> <keyword=explode>")
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

            assets.Add(StatusCopy("On Card Played Add Gearhammer To Hand", "On Card Played Add Shiv To Hand")
                .WithText("Add <{a}> {0} to your hand")
                .WithTextInsert("<card=autumnmooncat.wildfrost.spirefrost.shiv>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Instant Summon Shiv In Hand");
                })
            );

            assets.Add(StatusCopy("Instant Summon Gearhammer In Hand", "Instant Summon Shiv In Hand")
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantSummon>(data =>
                {
                    data.targetSummon = (StatusEffectSummon)TryGet<StatusEffectData>("Summon Shiv");
                })
            );

            assets.Add(StatusCopy("Summon Gearhammer", "Summon Shiv")
                .SubscribeToAfterAllBuildEvent<StatusEffectSummon>(data =>
                {
                    data.summonCard = TryGet<CardData>("shiv");
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

            assets.Add(new StatusEffectDataBuilder(MainModFile.instance)
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

            assets.Add(StatusCopy("While Active Reduce Attack To Enemies (No Ping, No Desc)", "While Active Reduce Attack To Enemies (With Desc)")
                .WithText("While active, reduce <keyword=attack> of all enemies by <{a}>")
            );

            assets.Add(StatusCopy("Increase Effects", "Increase Effects (With Desc)")
                .WithText("Boost the target's effects by <{a}>")
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantIncreaseEffects>(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintCanBeBoosted>()
                    };
                })
            );

            assets.Add(StatusCopy("On Card Played Add Gearhammer To Hand", "On Card Played Do Discovery Toolbox")
                .WithText("Choose 1 of <{a}> random <Items> to add to your hand")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("STS Discovery Toolbox");
                })
            );

            assets.Add(StatusCopy("Instant Summon Gunk In Hand", "Instant Summon Chosen Card")
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantSummon>(data =>
                {
                    data.targetSummon = (StatusEffectSummon)TryGet<StatusEffectData>("Summon Chosen Card");
                })
            );

            assets.Add(StatusCopy("Summon Gearhammer", "Summon Chosen Card")
                .SubscribeToAfterAllBuildEvent<StatusEffectSummon>(data =>
                {
                    data.summonCard = null;
                })
            );

            assets.Add(new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectDiscovery>("STS Discovery Toolbox")
                .WithText("")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectDiscovery>(data =>
                {
                    MainModFile.instance.predicateReferences.Add(data.name, obj => obj is CardData cardData && cardData.IsItem);
                    data.source = StatusEffectDiscovery.CardSource.Custom;
                    data.title = LocalizationHelper.GetCollection("UI Text", SystemLanguage.English).GetString(SpirefrostStrings.ToolboxTitle);
                })
            );

            assets.Add(StatusCopy("Set Attack", "Set Attack Of Card In Hand")
                .WithText("Set <keyword=attack> of a card in your hand to <{a}>")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantSetAttack>(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintDoesDamage>()
                    };
                })
            );

            assets.Add(new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectEquipMask>("STS Equip Mask")
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectEquipMask>(data =>
                {
                    
                })
            );

            assets.Add(new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectTempConvertAttackToYPreTrigger>("Attack Above X Counts As Frenzy")
                .WithText("<keyword=attack> above <{a}> counts as <keyword=frenzy>")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectTempConvertAttackToYPreTrigger>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("MultiHit");
                    data.oncePerTurn = true;
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintDoesDamage>()
                    };
                })
            );

            assets.Add(new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectLessonLearned>("Lesson Learned")
                .WithText("At the end of combat, add +{a}<keyword=attack> to a random <Item> in your deck")
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectLessonLearned>(data =>
                {
                    
                })
            );

            assets.Add(StatusCopy("On Card Played Add Zoomlin To Random Card In Hand", "On Card Played Add Zoomlin To Random Attack In Hand")
                .WithText("Add <keyword=zoomlin> to a random <Item> with <keyword=attack> in your hand")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    TargetConstraintAnd damageItem = ScriptableObject.CreateInstance<TargetConstraintAnd>();
                    damageItem.constraints = new TargetConstraint[] {
                        ScriptableObject.CreateInstance<TargetConstraintDoesDamage>(),
                        ScriptableObject.CreateInstance<TargetConstraintIsItem>()
                    };
                    data.applyConstraints = new TargetConstraint[]
                    {
                        damageItem
                    };
                })
            );

            assets.Add(StatusCopy("On Turn Apply Snow To Enemies", "On Turn Judge Enemies")
                .WithText("Set the <keyword=health> of all enemies with <{a}> or less to <0>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnTurn>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("STS Judgement");
                    data.applyConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintHasHealth>()
                    };
                })
            );

            assets.Add(new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectJudgement>("STS Judgement")
                .WithText("If the target has <{a}> or less <keyword=health>, set their <keyword=health> to 0")
                .SubscribeToAfterAllBuildEvent<StatusEffectJudgement>(data =>
                {
                    
                })
            );

            assets.Add(StatusCopy("On Card Played Add Zoomlin To Random Card In Hand", "On Card Played Apply Double Tap To Random Attack In Hand")
                .WithText("Apply <{a}><keyword=autumnmooncat.wildfrost.spirefrost.stsdoubletap> to a random <Item> with <keyword=attack> in your hand")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("STS Double Tap");
                    data.applyConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintDoesDamage>(),
                        ScriptableObject.CreateInstance<TargetConstraintIsItem>()
                    };
                })
            );

            assets.Add(StatusCopy("On Card Played Damage To Self", "On Card Played Combust Enemies")
                .WithText("Deal <{a}> damage to all enemies")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Enemies;
                })
            );

            assets.Add(StatusCopy("Bonus Damage Equal To Gold Factor 0.02", "Bonus Damage Equal To Skills In Hand")
                .WithText("Deal additional damage equal to <Items> in hand without <keyword=attack>")
                .SubscribeToAfterAllBuildEvent<StatusEffectBonusDamageEqualToX>(data =>
                {
                    ScriptableSkillsInHand skills = ScriptableObject.CreateInstance<ScriptableSkillsInHand>();
                    data.scriptableAmount = skills;
                })
            );

            assets.Add(StatusCopy("On Card Played Apply Attack To Self", "On Card Played Gain Random Charm")
                .WithText("Gain a random <Charm> for the rest of combat")
                .WithStackable(false)
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Instant Apply Random Charm");
                })
            );

            assets.Add(new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyRandomCharm>("Instant Apply Random Charm")
                .WithText("Apply a random <Charm>")
                .WithStackable(false)
                .WithCanBeBoosted(false)
            );

            assets.Add(StatusCopy("On Kill Apply Attack To Self", "On Kill Reduce Max Counter")
                .WithText("On kill, reduce <keyword=counter> by <{a}>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnKill>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Reduce Max Counter");
                })
            );

            assets.Add(StatusCopy("On Card Played Add Zoomlin To Random Card In Hand", "On Card Played Apply Amplify To Random Item In Hand")
                .WithText("Apply <{a}><keyword=autumnmooncat.wildfrost.spirefrost.stsamplify> to a random <Item> in your hand")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("STS Amplify");
                    data.applyConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintCanBeBoosted>(),
                        ScriptableObject.CreateInstance<TargetConstraintIsItem>()
                    };
                })
            );

            assets.Add(StatusCopy("On Card Played Lose Health", "On Card Played Lose Health Self")
                .WithText("Lose <{a}><keyword=health>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>("Instant Lose Health");
                    data.doPing = true;
                })
            );

            assets.Add(new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectInstantLoseHealth>("Instant Lose Health")
                .WithText("Reduce current <keyword=health> by <{a}>")
            );

            assets.Add(StatusCopy("When Hit Apply Snow To Attacker", "When Hit Increase Attacker Counter")
                .WithText("When hit, increase attacker's <keyword=counter> by {a}")
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenHit>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Increase Max Counter Adjusted");
                })
            );

            assets.Add(new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectInstantIncreaseMaxCounterAdjusted>("Increase Max Counter Adjusted")
                .WithCanBeBoosted(true)
                .WithOffensive(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantIncreaseMaxCounterAdjusted>(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintMaxCounterMoreThan>(),
                    };
                })
            );
        }

        private static void CreateKeywords()
        {
            // KEYWORDS
            // Keywords without pictures must use .WithShowName(true)
            assets.Add(new KeywordDataBuilder(MainModFile.instance)
                .Create("stsregen")
                .WithTitle("Regen")
                .WithDescription("Heals health every turn | Counts down every turn")
                .WithTitleColour(new Color(0.5f, 1.0f, 0.5f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.49f, 0.99f, 0.49f))
                .WithCanStack(true)
            );

            assets.Add(new KeywordDataBuilder(MainModFile.instance)
                .Create("stsvuln")
                .WithTitle("Vulnerable")
                .WithDescription("Increases damage taken by 50% for each Vulnerable | Clears after taking damage")
                .WithTitleColour(new Color(0.8f, 0.4f, 0.4f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.79f, 0.39f, 0.39f))
                .WithCanStack(true)
            );

            assets.Add(new KeywordDataBuilder(MainModFile.instance)
                .Create("stsweak")
                .WithTitle("Weakened")
                .WithDescription("Halves damage dealt | Counts down after triggering")
                .WithTitleColour(new Color(0.7f, 1.0f, 0.75f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.69f, 0.99f, 0.74f))
                .WithCanStack(true)
            );

            assets.Add(new KeywordDataBuilder(MainModFile.instance)
                .Create("stsamplify")
                .WithTitle("Amplify")
                .WithDescription("Increases target's effects | Clears after triggering")
                .WithTitleColour(new Color(0.25f, 0.75f, 0.85f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.24f, 0.74f, 0.84f))
                .WithCanStack(true)
            );

            assets.Add(new KeywordDataBuilder(MainModFile.instance)
                .Create("stsdoubletap")
                .WithTitle("Double Tap")
                .WithDescription("Trigger additional times | Clears after triggering")
                .WithTitleColour(new Color(0.55f, 0.1f, 0.1f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.54f, 0.09f, 0.09f))
                .WithCanStack(true)
            );

            assets.Add(new KeywordDataBuilder(MainModFile.instance)
                .Create("stsflight")
                .WithTitle("Flight")
                .WithDescription("Halves damage taken | Counts down after taking damage")
                .WithTitleColour(new Color(0.7f, 0.8f, 0.9f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.69f, 0.79f, 0.89f))
                .WithCanStack(true)
            );

            assets.Add(new KeywordDataBuilder(MainModFile.instance)
                .Create("stsritual")
                .WithTitle("Ritual")
                .WithDescription("When <Redraw Bell> is hit, gain <keyword=attack>")
                .WithTitleColour(new Color(0.5f, 0.8f, 1.0f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.49f, 0.79f, 0.99f))
                .WithCanStack(true)
            );

            assets.Add(new KeywordDataBuilder(MainModFile.instance)
                .Create("stsrollup")
                .WithTitle("Roll Up")
                .WithDescription("When hit, gain <keyword=shell> and lose Roll Up")
                .WithTitleColour(new Color(0.35f, 0.7f, 0.8f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.34f, 0.69f, 0.79f))
                .WithCanStack(true)
            );

            assets.Add(new KeywordDataBuilder(MainModFile.instance)
                .Create("stsmark")
                .WithTitle("Mark")
                .WithDescription("When applied, all enemies take damage equal to their Mark | Does not count down!")
                .WithTitleColour(new Color(0.4f, 0.7f, 0.7f))
                .WithBodyColour(new Color(1.0f, 1.0f, 1.0f))
                .WithNoteColour(new Color(0.39f, 0.69f, 0.69f))
                .WithCanStack(true)
            );
        }

        private static void CreateTraits()
        {
            // TRAITS
            /*assets.Add(new TraitDataBuilder(MainModFile.instance)
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

        private static void CreateIconBuilders()
        {
            // ICON BUILDERS
            // Default text color : 0.2471f, 0.1216f, 0.1647f, 1f
            assets.Add(new StatusIconBuilder(MainModFile.instance)
                .Create("STS Regen Icon", "spirefrost.stsregen", MainModFile.instance.ImagePath("Icons/RegenIcon.png"))
                .WithIconGroupName(StatusIconBuilder.IconGroups.health)
                .WithTextColour(new Color(0.2471f, 0.1216f, 0.1647f, 1f))
                .WithTextShadow(new Color(1.0f, 1.0f, 1.0f, 1.0f))
                .WithTextboxSprite()
                .WithKeywords("stsregen")
            );

            assets.Add(new StatusIconBuilder(MainModFile.instance)
                .Create("STS Vuln Icon", "spirefrost.stsvuln", MainModFile.instance.ImagePath("Icons/VulnIcon.png"))
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

            assets.Add(new StatusIconBuilder(MainModFile.instance)
                .Create("STS Weak Icon", "spirefrost.stsweak", MainModFile.instance.ImagePath("Icons/WeakIcon.png"))
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

            assets.Add(new StatusIconBuilder(MainModFile.instance)
                .Create("STS Amplify Icon", "spirefrost.stsamplify", MainModFile.instance.ImagePath("Icons/AmplifyIcon.png"))
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

            assets.Add(new StatusIconBuilder(MainModFile.instance)
                .Create("STS Double Tap Icon", "spirefrost.stsdoubletap", MainModFile.instance.ImagePath("Icons/DoubleTapIcon.png"))
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

            assets.Add(new StatusIconBuilder(MainModFile.instance)
                .Create("STS Flight Icon", "spirefrost.stsflight", MainModFile.instance.ImagePath("Icons/FlightIcon.png"))
                .WithIconGroupName(StatusIconBuilder.IconGroups.health)
                .WithTextColour(new Color(0.2471f, 0.1216f, 0.1647f, 1f))
                .WithTextShadow(new Color(1.0f, 1.0f, 1.0f, 1.0f))
                .WithTextboxSprite()
                .WithKeywords("stsflight")
            );

            assets.Add(new StatusIconBuilder(MainModFile.instance)
                .Create("STS Ritual Icon", "spirefrost.stsritual", MainModFile.instance.ImagePath("Icons/RitualIcon.png"))
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

            assets.Add(new StatusIconBuilder(MainModFile.instance)
                .Create("STS Roll Up Icon", "spirefrost.stsrollup", MainModFile.instance.ImagePath("Icons/RollUpIcon.png"))
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

            assets.Add(new StatusIconBuilder(MainModFile.instance)
                .Create("STS Mark Icon", "spirefrost.stsmark", MainModFile.instance.ImagePath("Icons/MarkIcon.png"))
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

        private static void CreateLeaders()
        {
            // LEADERS
            assets.Add(new CardDataBuilder(MainModFile.instance)
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
                        int ability = new Vector2Int(0, 6).Random();
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

                            // Double Tap
                            case 5:
                                card.SetRandomHealth(9, 11);
                                card.SetRandomDamage(4, 5);
                                card.SetRandomCounter(5, 5);
                                card.SetRandomPassive("On Card Played Apply Double Tap To Random Attack In Hand", 1, 1);
                                break;

                            // Combust
                            case 6:
                                card.SetRandomHealth(10, 14);
                                card.SetRandomDamage(2, 3);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomPassive("On Card Played Lose Health Self", 1, 1);
                                card.SetRandomPassive("On Card Played Combust Enemies", 1, 1);
                                break;
                        }
                    };

                    data.createScripts = new CardScript[]
                    {
                        ironcladScript
                    };
                })
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
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
                        int ability = new Vector2Int(0, 6).Random();
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

                            // Grand Finale - Removed
                            /*case 2:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(3, 4);
                                card.SetRandomCounter(3, 4);
                                card.SetRandomPassive("On Hit Damage If Draw Pile Empty", 4, 4);
                                break;*/

                            // Blade Dance
                            case 2:
                                card.SetRandomHealth(8, 11);
                                card.SetRandomDamage(1, 2);
                                card.SetRandomCounter(3, 4);
                                card.SetRandomPassive("On Card Played Add Shiv To Hand", 2, 2);
                                break;

                            // Malaise
                            case 3:
                                card.SetRandomHealth(6, 9);
                                card.SetRandomDamage(3, 5);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomActive("STS Weak", 1, 1);
                                break;

                            // Caltrops
                            case 4:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(2, 3);
                                card.SetRandomCounter(3, 4);
                                card.SetRandomPassive("Teeth", 2, 3);
                                break;

                            // Flechettes
                            case 5:
                                card.SetRandomHealth(7, 9);
                                card.SetRandomDamage(2, 3);
                                card.SetRandomCounter(3, 4);
                                card.SetRandomPassive("Bonus Damage Equal To Skills In Hand", 1, 1);
                                break;

                            // Alchemize
                            case 6:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(3, 5);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomPassive("On Card Played Gain Random Charm", 1, 1);
                                break;
                        }
                    };

                    data.createScripts = new CardScript[]
                    {
                        silentScript
                    };
                })
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
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
                        int ability = new Vector2Int(0, 6).Random();
                        switch (ability)
                        {
                            // Lightning
                            case 0:
                                card.SetRandomHealth(7, 9);
                                card.SetRandomDamage(0, 1);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomPassive("On Turn Summon Lightning Orb", 1, 1);
                                break;

                            // Dark
                            case 1:
                                card.SetRandomHealth(5, 7);
                                card.SetRandomDamage(0, 1);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomPassive("On Turn Summon Dark Orb", 1, 1);
                                break;

                            // Plasma
                            case 2:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(5, 7);
                                card.SetRandomCounter(6, 7);
                                card.SetRandomPassive("On Turn Summon Plasma Orb", 1, 1);
                                break;

                            // Frost
                            case 3:
                                card.SetRandomHealth(6, 8);
                                card.SetRandomDamage(3, 5);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomPassive("On Turn Summon Frost Orb", 1, 1);
                                break;

                            // Claw
                            case 4:
                                card.SetRandomHealth(7, 9);
                                card.SetRandomDamage(3, 4);
                                card.SetRandomCounter(4, 4);
                                card.SetRandomPassive("When Item Played, Increase Its Attack", 1, 1);
                                break;

                            // Sunder 
                            case 5:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(4, 6);
                                card.SetRandomCounter(5, 5);
                                card.SetRandomPassive("On Kill Reduce Max Counter", 1, 1);
                                break;

                            // Amplify 
                            case 6:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(3, 4);
                                card.SetRandomCounter(4, 4);
                                card.SetRandomPassive("On Card Played Apply Amplify To Random Item In Hand", 1, 1);
                                break;
                        }
                    };

                    data.createScripts = new CardScript[]
                    {
                        defectScript
                    };
                })
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
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
                        int ability = new Vector2Int(0, 6).Random();
                        switch (ability)
                        {
                            // Swivel
                            case 0:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(4, 6);
                                card.SetRandomCounter(5, 5);
                                card.SetRandomPassive("On Card Played Add Zoomlin To Random Attack In Hand", 1, 1);
                                break;

                            // Pressure Points
                            case 1:
                                card.SetRandomHealth(5, 7);
                                card.SetRandomDamage(0, 1);
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
                                card.traits = new List<CardData.TraitStacks> { TStack("Draw", 2) };
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

                            // Lesson Learned
                            case 5:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(4, 6);
                                card.SetRandomCounter(3, 4);
                                card.SetRandomPassive("Lesson Learned", 1, 1);
                                break;

                            // Judgement
                            case 6:
                                card.SetRandomHealth(7, 9);
                                card.SetRandomDamage(4, 6);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomPassive("On Turn Judge Enemies", 2, 2);
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

        private static void CreateCompanions()
        {
            // PETS
            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateUnit("louse", "Lousie")
                .SetSprites("Units/Louse.png", "Units/LouseBG.png")
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
            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateUnit("centurion", "Centurion")
                .SetSprites("Units/Centurion.png", "Units/CenturionBG.png")
                .SetStats(10, 3, 4)
                .WithValue(50)
                .SetTraits(TStack("Frontline", 1))
                .SetStartWithEffect(SStack("On Turn Apply Shell To Allies", 1))
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateUnit("nob", "Nob")
                .SetSprites("Units/Nob.png", "Units/NobBG.png")
                .SetStats(12, 3, 5)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    MainModFile.instance.maskedSpries.Add(data.name, MainModFile.instance.ImagePath("Units/NobMask.png").ToSprite());
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateUnit("lagavulin", "Lagavulin")
                .SetSprites("Units/Lagavulin.png", "Units/LagavulinBG.png")
                .SetStats(8, 4, 0)
                .WithValue(50)
                .SetTraits(TStack("Smackback", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    MainModFile.instance.maskedSpries.Add(data.name, MainModFile.instance.ImagePath("Units/LagavulinMask.png").ToSprite());
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Shell", 4),
                        SStack("When Hit Reduce Attack To Attacker", 1)
                    };
                })
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateUnit("slaver", "Slaver")
                .SetSprites("Units/Slaver.png", "Units/SlaverBG.png")
                .SetStats(6, 3, 5)
                .WithValue(50)
                .SetTraits(TStack("Longshot", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Hit Increase Counter", 2)
                    };
                })
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateUnit("chosen", "Chosen")
                .SetSprites("Units/Chosen.png", "Units/ChosenBG.png")
                .SetStats(11, 1, 3)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("MultiHit", 1),
                        SStack("On Turn Apply Amplify AllyBehind", 1)
                    };
                })
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
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
            assets.Add(new CardDataBuilder(MainModFile.instance)
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
                        SStack("When Hit Increase Attacker Counter", 1)
                    };
                })
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateUnit("exploder", "Exploder")
                .SetSprites("Units/Exploder.png", "Units/ExploderBG.png")
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
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
                        SStack("While Active Reduce Attack To Enemies (With Desc)", 2)
                    };
                })
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateUnit("bookofstabbing", "Book of Stabbing")
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateUnit("spirespear", "Spire Spear")
                .SetSprites("Units/SpireSpear.png", "Units/SpireSpearBG.png")
                .SetStats(null, 2, 0)
                .WithCardType("Clunker")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Scrap", 3),
                        SStack("MultiHit", 2),
                        SStack("When Hit Trigger To Self", 1)
                    };
                })
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
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
                        SStack("Bonus Damage Equal To Scrap On Board", 1)
                    };
                })
            );
        }

        private static void CreateSummons()
        {
            // SUMMONS
            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateUnit("lightningorb", "Lightning")
                .SetSprites("Summons/LightningOrb.png", "Summons/LightningOrbBG.png")
                .SetStats(2, 3, 1)
                .WithCardType("Summoned")
                .SetTraits(TStack("Aimless", 1))
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateUnit("plasmaorb", "Plasma")
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateUnit("frostorb", "Frost")
                .SetSprites("Summons/FrostOrb.png", "Summons/FrostOrbBG.png")
                .SetStats(2, 0, 1)
                .WithCardType("Summoned")
                .SetAttackEffect(SStack("Frost", 1))
            );
        }

        private static void CreateItems()
        {
            // STARTERS
            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateItem("shuriken", "Shuriken")
                .SetSprites("Items/Shuriken.png", "Items/ShurikenBG.png")
                .WithValue(10)
                .SetDamage(2)
                .SetStartWithEffect(SStack("On Hit Damage Damaged Target", 1))
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateItem("shovel", "Shovel")
                .SetSprites("Items/Shovel.png", "Items/ShovelBG.png")
                .WithValue(30)
                .SetDamage(0)
                .SetAttackEffect(SStack("Snow", 2))
                .SetTraits(TStack("Draw", 1))
                .WithFlavour("Diggy diggy hole")
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateItem("sundial", "Sundial")
                .SetSprites("Items/Sundial.png", "Items/SundialBG.png")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Reduce Counter", 1),

                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Card Played Increase Counter To RandomEnemy", 1)
                    };
                })
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
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
            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateItem("handdrill", "Hand Drill")
                .SetSprites("Items/HandDrill.png", "Items/HandDrillBG.png")
                .WithValue(50)
                .SetDamage(2)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Vuln", 2)
                    };
                })
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateItem("markofpain", "Mark Of Pain")
                .SetSprites("Items/MarkOfPain.png", "Items/MarkOfPainBG.png")
                .WithValue(50)
                .SetDamage(0)
                .SetTraits(TStack("Barrage", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Vuln", 1)
                    };
                })
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateItem("wristblade", "Wrist Blade")
                .SetSprites("Items/WristBlade.png", "Items/WristBladeBG.png")
                .WithValue(55)
                .SetDamage(1)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Weak", 1)
                    };
                })
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateItem("wingboots", "Wing Boots")
                .SetSprites("Items/WingBoots.png", "Items/WingBootsBG.png")
                .WithValue(45)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Flight", 1)
                    };
                })
            );

            /*assets.Add(new CardDataBuilder(MainModFile.instance)
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
            );*/

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateItem("battery", "Nuclear Battery")
                .SetSprites("Items/Battery.png", "Items/BatteryBG.png")
                .WithValue(60)
                .SetTraits(TStack("Consume", 1))
                .CanPlayOnHand(true)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Increase Effects (With Desc)", 1)
                    };
                })
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateItem("pocketwatch", "Pocketwatch")
                .SetSprites("Items/Pocketwatch.png", "Items/PocketwatchBG.png")
                .WithValue(40)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Double Tap", 1),
                        SStack("Increase Counter", 1)
                    };
                })
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateItem("fusionhammer", "Fusion Hammer")
                .SetSprites("Items/FusionHammer.png", "Items/FusionHammerBG.png")
                .WithValue(45)
                .SetDamage(1)
                .SetTraits(TStack("Barrage", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Amplify", 1)
                    };
                })
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateItem("lantern", "Eerie Lantern")
                .SetSprites("Items/Lantern.png", "Items/LanternBG.png")
                .WithValue(45)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Amplify", 2),
                        SStack("Increase Counter", 1)
                    };
                })
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateItem("icecream", "Ice Cream")
                .SetSprites("Items/IceCream.png", "Items/IceCreamBG.png")
                .WithValue(60)
                .SetDamage(0)
                .SetTraits(TStack("Consume", 1))
                .SetAttackEffect(SStack("Reduce Max Counter", 1), SStack("Snow", 2))
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateItem("bandages", "Tough Bandages")
                .SetSprites("Items/Bandages.png", "Items/BandagesBG.png")
                .WithValue(40)
                .SetAttackEffect(SStack("Increase Max Health", 1), SStack("Shell", 3))
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateItem("callingbell", "Calling Bell")
                .SetSprites("Items/CallingBell.png", "Items/CallingBellBG.png")
                .WithValue(55)
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

            assets.Add(new CardDataBuilder(MainModFile.instance)
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
            
            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateItem("toolbox", "Toolbox")
                .SetSprites("Items/Toolbox.png", "Items/ToolboxBG.png")
                .WithValue(50)
                .SetTraits(TStack("Consume", 1), TStack("Zoomlin", 1))
                .CanPlayOnHand(true)
                .NeedsTarget(false)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Card Played Do Discovery Toolbox", 3)
                    };
                })
            );
            
            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateItem("boot", "Boot")
                .SetSprites("Items/Boot.png", "Items/BootBG.png")
                .WithValue(55)
                .SetTraits(TStack("Consume", 1))
                .CanPlayOnHand(true)
                .CanPlayOnBoard(false)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Set Attack Of Card In Hand", 5)
                    };
                })
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateItem("cultistmask", "Cultist Mask")
                .SetSprites("Items/CultistMask.png", "Items/BlueCandleBG.png")
                .WithValue(60)
                .SetTraits(TStack("Consume", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Ritual", 1),
                        SStack("STS Equip Mask", 1)
                        //SStack("Reduce Max Health", 3)
                    };
                })
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateItem("kunai", "Kunai")
                .SetSprites("Items/Kunai.png", "Items/KunaiBG.png")
                .WithValue(50)
                .SetDamage(4)
                .WithFlavour("I am the era.")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Attack Above X Counts As Frenzy", 1)
                    };
                })
            );

            // TOKENS
            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateItem("holywater", "Holy Water")
                .SetSprites("Items/HolyWater.png", "Items/HolyWaterBG.png")
                .WithValue(50)
                .SetAttackEffect(SStack("Reduce Counter", 3))
                .SetTraits(TStack("Consume", 1))
            );

            assets.Add(new CardDataBuilder(MainModFile.instance)
                .CreateItem("shiv", "Shiv")
                .SetSprites("Items/Shiv.png", "Items/ShivBG.png")
                .WithValue(50)
                .SetDamage(2)
                .SetTraits(TStack("Consume", 1), TStack("Zoomlin", 1))
            );
        }

        private static void CreateCharms()
        {
            // CHARMS
            //TargetConstraintHasHealth hasHealth = ScriptableObject.CreateInstance<TargetConstraintHasHealth>();
            TargetConstraintCanBeHit canBeHit = ScriptableObject.CreateInstance<TargetConstraintCanBeHit>();
            TargetConstraintAttackMoreThan moreThan0Attack = ScriptableObject.CreateInstance<TargetConstraintAttackMoreThan>();
            moreThan0Attack.value = 0;
            TargetConstraintIsUnit isUnit = ScriptableObject.CreateInstance<TargetConstraintIsUnit>();
            assets.Add(new CardUpgradeDataBuilder(MainModFile.instance)
                .Create("CultistPotionCharm")
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/CultistCharm.png")
                .WithTitle("Cultist Potion")
                .WithText($"Start with <1><keyword=autumnmooncat.wildfrost.spirefrost.stsritual>\nReduce <keyword=attack> by <1>")
                .WithTier(2)
                .ChangeDamage(-1)
                .SetConstraints(moreThan0Attack, isUnit)
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
            assets.Add(new CardUpgradeDataBuilder(MainModFile.instance)
                .Create("FearPotionCharm")
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/FearCharm.png")
                .WithTitle("Fear Potion")
                .WithText($"Apply <2><keyword=autumnmooncat.wildfrost.spirefrost.stsvuln>")
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
            /*assets.Add(new CardUpgradeDataBuilder(MainModFile.instance)
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
            );*/

            assets.Add(new CardUpgradeDataBuilder(MainModFile.instance)
                .Create("FairyPotionCharm")
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/FairyCharm.png")
                .WithTitle("Fairy in a Bottle")
                .WithText($"Start with <2><keyword=autumnmooncat.wildfrost.spirefrost.stsflight>")
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

            assets.Add(new CardUpgradeDataBuilder(MainModFile.instance)
                .Create("WeakPotionCharm")
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/WeaknessCharm.png")
                .WithTitle("Weak Potion")
                .WithText($"Apply <1><keyword=autumnmooncat.wildfrost.spirefrost.stsweak>")
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

            TargetConstraintOr canAct = ScriptableObject.CreateInstance<TargetConstraintOr>();
            canAct.constraints = new TargetConstraint[] {
                hasCounter,
                hasReaction
            };
            assets.Add(new CardUpgradeDataBuilder(MainModFile.instance)
                .Create("MiraclePotionCharm")
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/MiracleCharm.png")
                .WithTitle("Bottled Miracle")
                .WithText($"When hit, count down <keyword=counter> by <1>")
                .WithTier(2)
                .SetConstraints(isUnit, canAct)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.effects = new CardData.StatusEffectStacks[]
                    {
                        SStack("When Hit Reduce Counter To Self", 1)
                    };
                })
            );

            assets.Add(new CardUpgradeDataBuilder(MainModFile.instance)
                .Create("IronPotionCharm")
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/HeartOfIronCharm.png")
                .WithTitle("Heart of Iron")
                .WithText($"When <Redraw Bell> is hit, gain <1><keyword=shell>")
                .WithTier(1)
                .ChangeCounter(2)
                .SetConstraints(canBeHit)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.effects = new CardData.StatusEffectStacks[]
                    {
                        SStack("When Redraw Hit Apply Shell To Self", 1)
                    };
                })
            );

            assets.Add(new CardUpgradeDataBuilder(MainModFile.instance)
                .Create("EntropicBrewCharm")
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/EntropicCharm.png")
                .WithTitle("Entropic Brew")
                .WithText($"Apply <3> other random <Charms> to this card\nThey do not take up charm slots")
                .WithTier(2)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    CardScriptRunnable entropicScript = ScriptableObject.CreateInstance<CardScriptRunnable>();
                    entropicScript.runnable = card =>
                    {
                        List<CardUpgradeData> validUpgrades = new List<CardUpgradeData>();
                        foreach(CardUpgradeData upgrade in AddressableLoader.GetGroup<CardUpgradeData>("CardUpgradeData"))
                        {
                            if (upgrade.type == CardUpgradeData.Type.Charm && upgrade.tier >= 0 && !(upgrade.name.Equals("autumnmooncat.wildfrost.spirefrost.EntropicBrewCharm")) && upgrade.CanAssign(card))
                            {
                                validUpgrades.Add(upgrade);
                            }
                        }
                        int applyAmount = Math.Min(3, validUpgrades.Count);
                        for (int i = 0; i < applyAmount; i++)
                        {
                            CardUpgradeData applyMe = validUpgrades.TakeRandom().Clone();
                            applyMe.takeSlot = false;
                            applyMe.Assign(card);
                        }
                    };
                    data.scripts = new CardScript[]
                    {
                        entropicScript
                    };
                })
            );

            assets.Add(new CardUpgradeDataBuilder(MainModFile.instance)
                .Create("DuplicationCharm")
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/DuplicationCharm.png")
                .WithTitle("Duplication Potion")
                .WithText($"Create a copy of an <Item>\nDoes not take up a charm slot")
                .WithTier(2)
                .SetConstraints(isItem)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    CardScriptRunnable duplicationScript = ScriptableObject.CreateInstance<CardScriptRunnable>();
                    duplicationScript.runnable = card =>
                    {
                        if (!MainModFile.instance.looping)
                        {
                            // Safety Lock
                            MainModFile.instance.looping = true;

                            System.Collections.IEnumerator logic = SpirefrostUtils.DuplicationLogic(data, card);

                            while (logic.MoveNext())
                            {
                                object n = logic.Current;
                                Debug.Log("Duplication Script: "+n);
                            }
                        }
                    };
                    data.scripts = new CardScript[]
                    {
                        duplicationScript
                    };
                    //data.takeSlot = false;
                })
            );
        }
    }
}
