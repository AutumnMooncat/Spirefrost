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
using System.Net.NetworkInformation;


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
                        inventory.deck.list = DataList<CardData>("SnowGlobe", "Sword", "Gearhammer", "Dart", "EnergyDart", "SunlightDrum", "Junkhead", "IceDice").ToList();
                        //inventory.upgrades.Add(TryGet<CardUpgradeData>("CardUpgradeCritical"));
                        data.startingInventory = inventory;

                        RewardPool unitPool = CreateRewardPool("DrawUnitPool", "Units", DataList<CardData>(
                            "NakedGnome", "GuardianGnome", "Havok",
                            "Gearhead", "Bear", "TheBaker",
                            "Pimento", "Pootie", "Tusk",
                            "Ditto", "Flash", "TinyTyko"));

                        RewardPool itemPool = CreateRewardPool("DrawItemPool", "Items", DataList<CardData>(
                            "ShellShield", "StormbearSpirit", "PepperFlag", "SporePack", "Woodhead",
                            "BeepopMask", "Dittostone", "Putty", "Dart", "SharkTooth",
                            "Bumblebee", "Badoo", "Juicepot", "PomDispenser", "LuminShard",
                            "Wrenchy", "Vimifier", "OhNo", "Madness", "Joob"));

                        RewardPool charmPool = CreateRewardPool("DrawCharmPool", "Charms", DataList<CardUpgradeData>(
                            "CardUpgradeTrash",
                            "CardUpgradeInk", "CardUpgradeOverload",
                            "CardUpgradeMime", "CardUpgradeShellBecomesSpice",
                            "CardUpgradeAimless"));

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
                        new TargetConstraintHasHealth(),
                        new TargetConstraintIsAlive()
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
                        new TargetConstraintCanBeHit()
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
                    data.targetConstraints = new TargetConstraint[]
                    {
                        new TargetConstraintCanBeBoosted(),
                        new TargetConstraintIsUnit(),
                        new TargetConstraintOr()
                        {
                            constraints = new TargetConstraint[]
                            {
                                new TargetConstraintHasReaction(),
                                new TargetConstraintMaxCounterMoreThan()
                                {
                                    moreThan = 0
                                }
                            }
                        }
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
                        new TargetConstraintCanBeHit()
                    };
                })
                .Subscribe_WithStatusIcon("STS Flight Icon")
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
                        new TargetConstraintDoesDamage()
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
                        new TargetConstraintCanBeHit()
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
                .WithTextInsert("<card=autumnmooncat.wildfrost.spirefrost.lightningOrb>")
                .SubscribeToAfterAllBuildEvent<StatusEffectSummon>(data =>
                {
                    data.summonCard = TryGet<CardData>("lightningOrb");
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
                .WithIconGroupName(StatusIconBuilder.IconGroups.counter)
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
        }

        private void CreateLeaders()
        {
            // LEADERS
            assets.Add(
                new CardDataBuilder(this).CreateUnit("ironclad", "Ironclad") //Internally the card's name will be "[GUID].shadeSerpent". In-game, it will be "Shade Serpent".
                .SetSprites("Ironclad.png", "IroncladBG.png")
                .SetStats(8, 3, 4)
                .WithCardType("Leader") //All companions are "Friendly". Also, this line is not necessary since CreateUnit already sets the cardType to "Friendly".
                                        //.WithFlavour("I don't have an ability yet :/")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.createScripts = new CardScript[]  //These scripts run when right before Events.OnCardDataCreated
                    {
                        GiveUpgrade(),                     //By our definition, no argument will give a crown
                        AddRandomHealth(-2,2),
                        AddRandomDamage(-1,1),
                        AddRandomCounter(-1,1)
                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("When Card Destroyed, Gain Shell", 2)
                    };
                })
                .AddPool("SnowUnitPool") //This puts Shade Serpent in the Shademancer pools. Other choices were "GeneralUnitPool", "SnowUnitPool", "BasicUnitPool", and "ClunkUnitPool".
            );

            assets.Add(
                new CardDataBuilder(this).CreateUnit("silent", "Silent") //Internally the card's name will be "[GUID].shadeSerpent". In-game, it will be "Shade Serpent".
                .SetSprites("Silent.png", "SilentBG.png")
                .SetStats(6, 2, 3)
                .WithCardType("Leader") //All companions are "Friendly". Also, this line is not necessary since CreateUnit already sets the cardType to "Friendly".
                                        //.WithFlavour("I don't have an ability yet :/")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("When Enemy Is Hit By Item Apply Reduce Their Health", 1)
                    };
                })
                .AddPool("SnowUnitPool") //This puts Shade Serpent in the Shademancer pools. Other choices were "GeneralUnitPool", "SnowUnitPool", "BasicUnitPool", and "ClunkUnitPool".
            );

            assets.Add(
                new CardDataBuilder(this).CreateUnit("defect", "Defect") //Internally the card's name will be "[GUID].shadeSerpent". In-game, it will be "Shade Serpent".
                .SetSprites("Defect.png", "DefectBG.png")
                .SetStats(7, null, 4)
                .WithCardType("Leader") //All companions are "Friendly". Also, this line is not necessary since CreateUnit already sets the cardType to "Friendly".
                                        //.WithFlavour("I don't have an ability yet :/")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Summon Lightning Orb", 1)
                    };
                })
                .AddPool("SnowUnitPool") //This puts Shade Serpent in the Shademancer pools. Other choices were "GeneralUnitPool", "SnowUnitPool", "BasicUnitPool", and "ClunkUnitPool".
            );

            assets.Add(
                new CardDataBuilder(this).CreateUnit("watcher", "Watcher") //Internally the card's name will be "[GUID].shadeSerpent". In-game, it will be "Shade Serpent".
                .SetSprites("Watcher.png", "WatcherBG.png")
                .SetStats(7, 4, 5)
                .WithCardType("Leader") //All companions are "Friendly". Also, this line is not necessary since CreateUnit already sets the cardType to "Friendly".
                                        //.WithFlavour("I don't have an ability yet :/")
                .SetStartWithEffect(SStack("On Card Played Add Zoomlin To Random Card In Hand", 1))
                .AddPool("SnowUnitPool") //This puts Shade Serpent in the Shademancer pools. Other choices were "GeneralUnitPool", "SnowUnitPool", "BasicUnitPool", and "ClunkUnitPool".
            );
        }

        private void CreateCompanions()
        {
            // PETS

            // UNITS
        }

        private void CreateSummons()
        {
            // SUMMONS
            assets.Add(
                new CardDataBuilder(this).CreateUnit("lightningOrb", "Lightning") //Internally the card's name will be "[GUID].shadeSerpent". In-game, it will be "Shade Serpent".
                .SetSprites("LightningOrb.png", "LightningOrbBG.png")
                .SetStats(2, 2, 1)
                .WithCardType("Summoned") //All companions are "Friendly". Also, this line is not necessary since CreateUnit already sets the cardType to "Friendly".
                .WithFlavour("Zap")
                .SetTraits(TStack("Aimless", 1))
            );
        }

        private void CreateItems()
        {
            // ITEMS
            assets.Add(
                new CardDataBuilder(this).CreateItem("shovel", "Shovel")
                .SetSprites("Shovel.png", "ShovelBG.png")
                .SetDamage(0)
                .SetAttackEffect(SStack("Snow", 2))
                .SetTraits(TStack("Draw", 1))
                .WithFlavour("Diggy diggy hole")
            );

            assets.Add(
                new CardDataBuilder(this).CreateItem("sundial", "Sundial")
                .SetSprites("Sundial.png", "SundialBG.png")
                .SetAttackEffect(SStack("Reduce Counter", 1))
                //.SetTraits(TStack("Barrage", 1))
                .WithFlavour("Autumn hasnt coded this yet")
            );
        }

        private void CreateCharms()
        {
            // CHARMS
            assets.Add(
                new CardUpgradeDataBuilder(this)
                .Create("CardUpgradeTest1")                         //Internally named as CardUpgradeGlacial
                //.AddPool("GeneralCharmPool")                          //Adds the upgrade to the general pool
                .WithType(CardUpgradeData.Type.Charm)                 //Sets the upgrade to a charm (other choices are crowns and tokens)
                .WithImage("TestCharm.png")                        //Sets the image file path to "GlacialCharm.png". See below.
                .WithTitle("Test Charm")                           //Sets in-game name as Glacial Charm
                .WithText($"Testing: Gain 3 <keyword=autumnmooncat.wildfrost.spirefrost.stsregen> " +
                $"<keyword=autumnmooncat.wildfrost.spirefrost.stsvuln> <keyword=autumnmooncat.wildfrost.spirefrost.stsweak> " +
                $"<keyword=autumnmooncat.wildfrost.spirefrost.stsamplify> <keyword=autumnmooncat.wildfrost.spirefrost.stsdoubletap> " +
                $"<keyword=autumnmooncat.wildfrost.spirefrost.stsflight>") //Get allows me to skip the GUID. The Text class does not.
                                                                                               //IMPORTANT: if you did not heed the advice from before, the keyword name must be lowercase, so use .ToLower() to fix that.
                                                                                               //If you are having trouble, find your keyword via the Unity Explorer and verify its name. 
                .WithTier(2)                                          //Affects cost in shops
                .SetConstraints(new TargetConstraintDoesDamage(), new TargetConstraintCanBeHit())
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.effects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Regen", 3),
                        SStack("STS Vuln", 3),
                        SStack("STS Weak", 3),
                        SStack("STS Amplify", 3),
                        SStack("STS Double Tap", 3),
                        SStack("STS Flight", 3)
                    };
                })
            );
            
            assets.Add(
                new CardUpgradeDataBuilder(this)
                .Create("CardUpgradeTest2")
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("TestCharm.png")
                .WithTitle("Test Charm")
                .WithText($"Testing: Gain 2 <keyword=autumnmooncat.wildfrost.spirefrost.stsritual> <keyword=autumnmooncat.wildfrost.spirefrost.stsrollup>")
                .WithTier(2)
                .SetConstraints(new TargetConstraintDoesDamage(), new TargetConstraintCanBeHit())
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.effects = new CardData.StatusEffectStacks[]
                    {
                        SStack("STS Ritual", 2),
                        SStack("STS Roll Up", 2)
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

        internal CardScript GiveUpgrade(string name = "Crown") //Give a crown
        {
            CardScriptGiveUpgrade script = ScriptableObject.CreateInstance<CardScriptGiveUpgrade>(); //This is the standard way of creating a ScriptableObject
            script.name = $"Give {name}";                               //Name only appears in the Unity Inspector. It has no other relevance beyond that.
            script.upgradeData = TryGet<CardUpgradeData>(name);
            return script;
        }

        internal CardScript AddRandomHealth(int min, int max) //Boost health by a random amount
        {
            CardScriptAddRandomHealth health = ScriptableObject.CreateInstance<CardScriptAddRandomHealth>();
            health.name = "Random Health";
            health.healthRange = new Vector2Int(min, max);
            return health;
        }

        internal CardScript AddRandomDamage(int min, int max) //Boost damage by a ranom amount
        {
            CardScriptAddRandomDamage damage = ScriptableObject.CreateInstance<CardScriptAddRandomDamage>();
            damage.name = "Give Damage";
            damage.damageRange = new Vector2Int(min, max);
            return damage;
        }

        internal CardScript AddRandomCounter(int min, int max) //Increase counter by a random amount
        {
            CardScriptAddRandomCounter counter = ScriptableObject.CreateInstance<CardScriptAddRandomCounter>();
            counter.name = "Give Counter";
            counter.counterRange = new Vector2Int(min, max);
            return counter;
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
            hit.damage = Mathf.RoundToInt(hit.damage * (1 + (amountToClear * 0.5f)));
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
                amountRemoved = Mathf.RoundToInt(target.tempDamage.Value / 2f);
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
