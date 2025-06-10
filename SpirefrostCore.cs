using Deadpan.Enums.Engine.Components.Modding;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;                  // Declares TMP_SpriteAsset
using WildfrostHopeMod.Utils; // Creates TMP_SpriteAsset
using WildfrostHopeMod.VFX;   // Declares StatusIconBuilder
using Extensions = Deadpan.Enums.Engine.Components.Modding.Extensions;
using static Spirefrost.SpirefrostUtils;
using Spirefrost.Builders.Tribes;
using Spirefrost.Builders.StatusEffects.IconEffects;
using static Spirefrost.Patches.ConfigPatches;
using Spirefrost.Patches;
using Spirefrost.StatusEffects;


namespace Spirefrost
{
    public class MainModFile : WildfrostMod
    {
        public static MainModFile instance;
        private readonly Harmony HarmonySafetyInstance;
        private bool preLoaded;

        // This allows for icons in descriptions
        public override TMP_SpriteAsset SpriteAsset => spriteAsset;
        internal static TMP_SpriteAsset spriteAsset;

        internal Color rainbowColor = new Color(1f, 1f, 1f, 1f);

        internal Texture2D entropicOverlay;
        internal Texture2D entropicUnderlay;

        internal Texture2D duplicationOverlay;
        internal Texture2D duplicationUnderlay;

        internal Sprite wideCompanionFrame;
        internal Sprite wideCompanionFrameChisel;
        internal Sprite wideCompanionFrameGold;
        internal Sprite wideCompanionMask;
        internal Sprite wideCompanionChiseledMask;
        internal Sprite wideCompanionOutline;
        internal Sprite wideCompanionChiseledOutline;
        internal Sprite wideCompanionNameTag;
        internal Sprite wideCompanionChiseledNameTag;
        internal Sprite wideCompanionTextBox;

        internal GameObject managedObjects;
        internal GameObject tempObjects;

        internal Entity dummyEntity;

        internal bool updated;
        internal bool looping;

        internal Dictionary<string, Sprite> maskedSpries = new Dictionary<string, Sprite>();
        internal Dictionary<string, Predicate<object>> predicateReferences = new Dictionary<string, Predicate<object>>();

        internal Dictionary<PoolListType, List<WeightedString>> poolData = new Dictionary<PoolListType, List<WeightedString>>();

        public enum ReplaceType
        {
            Off,
            If_StS_Leader,
            On
        }

        //[ConfigItem(ReplaceType.Off, "Give <Junk> a \"fitting\" depricated relic image", "Replace Junk Image")]
        public ReplaceType junkReplace = ReplaceType.Off;

        [ConfigItem(false, "Should Cultist Mask play sfx", "Caw Caw?")]
        [ConfigManagerCallbackSetting("CawCawCallback")]
        [ConfigManagerOptionSetting(new string[] { "No", "Caw Caw!" }, new object[] { false, true })]
        public bool cawCaw = false;

        [ConfigItem(ReplaceType.If_StS_Leader, "Replace <sprite=demonize> with <sprite=spirefrost.stsvuln>", "Demonize is Vulnerable")]
        public ReplaceType vulnReplace = ReplaceType.If_StS_Leader;

        [ConfigItem(ReplaceType.If_StS_Leader, "Replace <sprite=frost> with <sprite=spirefrost.stsweak>", "Frost is Weak")]
        [ConfigManagerExclusiveSetting("shackledReplace", new object[] { ReplaceType.If_StS_Leader, ReplaceType.On }, ReplaceType.Off)]
        public ReplaceType weakReplace = ReplaceType.If_StS_Leader;

        [ConfigItem(ReplaceType.Off, "Replace <sprite=frost> with <sprite=spirefrost.stsshackled>", "Frost is Shackled")]
        [ConfigManagerExclusiveSetting("weakReplace", new object[] { ReplaceType.If_StS_Leader, ReplaceType.On }, ReplaceType.Off)]
        public ReplaceType shackledReplace = ReplaceType.Off;

        [ConfigItem(ReplaceType.If_StS_Leader, "Replace <sprite=shroom> with <sprite=spirefrost.stspoison>", "Shroom is Poison")]
        public ReplaceType poisonReplace = ReplaceType.If_StS_Leader;

        [ConfigItem(ReplaceType.If_StS_Leader, "Replace <sprite=teeth> with <sprite=spirefrost.ststhorns>", "Teeth is Thorns")]
        public ReplaceType thornsReplace = ReplaceType.If_StS_Leader;

        [ConfigItem(ReplaceType.If_StS_Leader, "Replace <sprite=spice> with <sprite=spirefrost.stsvigor>", "Spice is Vigor")]
        public ReplaceType vigorReplace = ReplaceType.If_StS_Leader;

        [ConfigItem(ReplaceType.If_StS_Leader, "Replace <sprite=haze> with <sprite=spirefrost.stsconfused>", "Haze is Confused")]
        public ReplaceType confusedReplace = ReplaceType.If_StS_Leader;

        internal static void CawCawCallback(object value)
        {
            if (value is bool val && val)
            {
                VFXMod.instance.SFX.TryPlaySoundFromPath(MainModFile.instance.ImagePath("SFX/CawCaw.ogg"));
            }
        }

        public enum PoolListType
        {
            Leaders,
            StarterItems,
            IroncladStarterItems,
            SilentStarterItems,
            DefectStarterItems,
            WatcherStarterItems,
            Items,
            IroncladItems,
            SilentItems,
            DefectItems,
            WatcherItems,
            Units,
            IroncladUnits,
            SilentUnits,
            DefectUnits,
            WatcherUnits,
            Charms,
            IroncladCharms,
            SilentCharms,
            DefectCharms,
            WatcherCharms
        }

        public MainModFile(string modDirectory) : base(modDirectory)
        { 
            instance = this;
            HarmonySafetyInstance = new Harmony(GUID+".safety");
        }

        public override string GUID => "autumnmooncat.wildfrost.spirefrost";
        public override string[] Depends => new string[] { "hope.wildfrost.vfx" };
        public override string Title => "Spirefrost";
        public override string Description => "Adds a new tribe featuring Characters and Monsters from Slay the Spire.\r\n\r\n[h1]New Leaders[/h1]\r\nAdds a new tribe with 4 new Leaders, each having 7 different configurations they can generate from, offering different stats and effects. Play as an Alchemize Silent and generate charms mid combat, or enjoy Judging enemies as Watcher! Leader effects are based on some of their more iconic cards from Slay the Spire.\r\n\r\n[h1]New Cards[/h1]\r\nMonsters, Relics, and Potions have been translated over to act as new Companions, Items, and Charms you can find while playing as the Spire tribe, many of which are leader specific, giving each leader their own unique flavour.\r\nIncludes a total of 40 new relic themed Items, 31 new Companions, 10 new Clunkers, 8 new Charms, and 1 new Pet!\r\n\r\n[h1]Vanilla Changes[/h1]\r\nSpirefrost makes a few adjustments to vanilla mechanics in order to make some of its new effects work. If the game is behaving slightly different than you would expect, check the following list to see if Spirefrost is responsible for the change:[list]\r\n[*] Frenzy now activates when additional triggers happen while the unit was already triggered (such as Nova losing Block on Teeth)\r\n[*] Cards now receive credit from kills while in the discard or draw piles (such as an Item applying Shroom that then kills after the turn phase)\r\n[*] The shove system has been rewritten to support Wide cards that take 2 slots in a row\r\n[*] Cleanse no longer removes temporary debuffs applied by While Active effects from other units (as you would otherwise just move the units to reapply the effect anyway)\r\n[*] When checking if an Item is valid to play on a entity, the current Attack stat of the card is checked as opposed to its base Attack\r\n[/list]\r\n\r\n[h1]Configurable Changes[/h1]\r\nBy default, while playing a run as a leader from the Spire tribe, certain status effects are changed to be their Slay the Spire power counterparts, such as Demonize being Vulnerable or Shroom being Poison. These changes are strictly cosmetic and no mechanical changes occur. These changes can be individually toggled in the mods config settings if you happen to have Config Manager installed!";

        public override void Load()
        {
            if (!preLoaded)
            {
                // The spriteAsset has to be defined before any icons are made!
                spriteAsset = HopeUtils.CreateSpriteAsset(Title);
                SpirefrostAssetHandler.CreateAssets();
                preLoaded = true;

                entropicOverlay = "Charms/EntropicCharmOverlay.png".ToNamedTex();
                entropicUnderlay = "Charms/EntropicCharmUnderlay.png".ToNamedTex();
                duplicationOverlay = "Charms/DuplicationCharmOverlay.png".ToNamedTex();
                duplicationUnderlay = "Charms/DuplicationCharmUnderlay.png".ToNamedTex();

                wideCompanionFrame = "Frames/WideCompanionFrame.png".ToNamedSprite();
                wideCompanionFrameChisel = "Frames/WideCompanionChiseledFrame.png".ToNamedSprite();
                wideCompanionFrameGold = "Frames/WideCompanionGoldenFrame.png".ToNamedSprite();
                wideCompanionMask = "Frames/WideCompanionFrameMask.png".ToNamedSprite();
                wideCompanionChiseledMask = "Frames/WideCompanionChiseledFrameMask.png".ToNamedSprite();
                wideCompanionOutline = "Frames/WideCompanionFrameOutline.png".ToNamedSprite();
                wideCompanionChiseledOutline = "Frames/WideCompanionChiseledFrameOutline.png".ToNamedSprite();
                wideCompanionNameTag = "Frames/WideCompanionNameTag.png".ToNamedSprite();
                wideCompanionChiseledNameTag = "Frames/WideCompanionChiseledNameTag.png".ToNamedSprite();
                wideCompanionTextBox = "Frames/WideCompanionTextBox.png".ToNamedSprite();

                WideUtils.CreateWideCardPools();

                // Add prevent death effects to kill list
                StatusEffectInstantKill kill = TryGet<StatusEffectData>("Kill") as StatusEffectInstantKill;
                if (kill)
                {
                    kill.statusesToClear = kill.statusesToClear.AddRangeToArray(new string[] { Intangible.FullID, Fading.FullID });
                }
            }

            // Let our sprites automatically show up for icon descriptions
            SpriteAsset.RegisterSpriteAsset();
            ApplySafetyPatches();
            base.Load();
            SpirefrostStrings.CreateLocalizedStrings();
            GameMode gameMode = TryGet<GameMode>("GameModeNormal"); //GameModeNormal is the standard game mode. 
            gameMode.classes = gameMode.classes.Append(TryGet<ClassData>(SpireTribe.ID)).ToArray();
            Events.OnEntityCreated += FixImage;
            Events.PostBattle += CleanUpBattleEnd;
            Events.OnBackToMainMenu += CleanUpTemp;
            Events.OnEntityChosen += PickupCheck;

            managedObjects = new GameObject(Title+".ManagedObjects");
            UnityEngine.Object.DontDestroyOnLoad(managedObjects);
            managedObjects.AddComponent<UpdateManager>();
            GameObject dummyRef = new GameObject("Dummy Entity", typeof(RectTransform), typeof(Entity));
            dummyRef.transform.SetParent(managedObjects.transform);
            dummyEntity = dummyRef.GetComponent<Entity>();
            dummyEntity.statusEffects = new List<StatusEffectData>();

            tempObjects = new GameObject(Title + ".TempObjects");
            UnityEngine.Object.DontDestroyOnLoad(tempObjects);
        }

        public override void Unload()
        {
            // Prevent our icons from accidentally showing up in descriptions when not loaded
            SpriteAsset.UnRegisterSpriteAsset();
            base.Unload();
            RemoveSafetyPatches();
            GameMode gameMode = TryGet<GameMode>("GameModeNormal");
            gameMode.classes = RemoveNulls(gameMode.classes); //Without this, a non-restarted game would crash on tribe selection
            UnloadFromClasses();
            Events.OnEntityCreated -= FixImage;
            Events.PostBattle -= CleanUpBattleEnd;
            Events.OnBackToMainMenu -= CleanUpTemp;
            Events.OnEntityChosen -= PickupCheck;
            managedObjects.Destroy();
            managedObjects = null;
            tempObjects.Destroy();
            tempObjects = null;
        }

        private void ApplySafetyPatches()
        {
            HarmonySafetyInstance.Patch(SafetyPatches.Module_GetAssembly, transpiler: new HarmonyMethod(SafetyPatches.Module_GetAssembly_Patch));
        }

        private void RemoveSafetyPatches()
        {
            HarmonySafetyInstance.Unpatch(SafetyPatches.Module_GetAssembly, SafetyPatches.Module_GetAssembly_Patch);
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

        //Credits to Hopeful for this method
        public override List<T> AddAssets<T, Y>()
        {
            if (SpirefrostAssetHandler.assets.OfType<T>().Any())
                Debug.LogWarning($"[{Title}] adding {typeof(Y).Name}s: {SpirefrostAssetHandler.assets.OfType<T>().Select(a => a._data.name).Join()}");
            return SpirefrostAssetHandler.assets.OfType<T>().ToList();
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

            List<FinalBossEffectSwapper> foundSwappers = new List<FinalBossEffectSwapper>();
            foreach (var item in GetAllNamedReferences(StatusEffectDataExtensions.SwapperKey)) 
            {
                if (item is FinalBossEffectSwapper swapper)
                {
                    foundSwappers.Add(swapper);
                }
            }
            BattleGenerationScriptFinalBoss scriptFinalBoss = TryGet<BattleData>("Final Boss").generationScript as BattleGenerationScriptFinalBoss;
            scriptFinalBoss.settings.effectSwappers = scriptFinalBoss.settings.effectSwappers.ToList().Except(foundSwappers).ToArray();
            foreach (var item in foundSwappers)
            {
                item.Destroy();
            }
            foundSwappers.Clear();

            FreeAllReferences();
        }

        internal T[] RemoveNulls<T>(T[] data) where T : DataFile
        {
            List<T> list = data.ToList();
            list.RemoveAll(x => x == null || x.ModAdded == this);
            return list.ToArray();
        }

        private void PickupCheck(Entity entity)
        {
            if (References.Player)
            {
                foreach (var item in entity.statusEffects)
                {
                    if (item is StatusEffectLinkedCard link)
                    {
                        for (int i = 0; i < item.count; i++)
                        {
                            References.Player.data.inventory.deck.Add(link.linkedCard.Clone(true));
                        }
                    }
                }
                entity.data.startWithEffects = entity.data.startWithEffects.ToList().Where(s => !(s.data is StatusEffectLinkedCard)).ToArray();
                if (entity.display is Card card)
                {
                    card.UpdateData(false);
                }
            }
        }

        private void CleanUpTemp()
        {
            tempObjects.Destroy();
            tempObjects = new GameObject(Title + ".TempObjects");
            UnityEngine.Object.DontDestroyOnLoad(tempObjects);
        }

        private void CleanUpBattleEnd(CampaignNode node)
        {
            CleanUpTemp();
        }

        //Remember to hook this method onto Events.OnEntityCreated in the Load/Unload (see Tutorial 1 or the full code for more details).
        private void FixImage(Entity entity)
        {
            if (entity.display is Card card && !card.hasScriptableImage) //These cards should use the static image
            {
                card.mainImage.gameObject.SetActive(true);               //And this line turns them on
            }
        }

        internal static string[] PoolToIDs(PoolListType type)
        {
            return instance.poolData.GetValueOrDefault(type, new List<WeightedString>()).Select(ws => ws.str).ToArray();
        }

        internal static void Print(string str)
        {
            System.Console.WriteLine("[Spirefrost] "+str);
        }
    }
}
