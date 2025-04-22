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


namespace Spirefrost
{
    public class MainModFile : WildfrostMod
    {
        public static MainModFile instance;
        private bool preLoaded;

        // This allows for icons in descriptions
        public override TMP_SpriteAsset SpriteAsset => spriteAsset;
        internal static TMP_SpriteAsset spriteAsset;

        internal Color rainbowColor = new Color(1f, 1f, 1f, 1f);

        internal Texture2D entropicOverlay;
        internal Texture2D entropicUnderlay;

        internal Texture2D duplicationOverlay;
        internal Texture2D duplicationUnderlay;

        internal GameObject managedObjects;
        internal GameObject tempObjects;

        internal bool updated;
        internal bool looping;

        internal Dictionary<string, Sprite> maskedSpries = new Dictionary<string, Sprite>();
        internal Dictionary<string, Predicate<object>> predicateReferences = new Dictionary<string, Predicate<object>>();

        internal Dictionary<PoolListType, List<WeightedString>> poolData = new Dictionary<PoolListType, List<WeightedString>>();

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
            Charms,
            IroncladCharms,
            SilentCharms,
            DefectCharms,
            WatcherCharms
        }

        public MainModFile(string modDirectory) : base(modDirectory)
        { 
            instance = this;
        }

        public override string GUID => "autumnmooncat.wildfrost.spirefrost";
        public override string[] Depends => new string[] { "hope.wildfrost.vfx" };
        public override string Title => "Spirefrost";
        public override string Description => "Adds Slay the Spire content to Wildfrost.";

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
            }

            // Let our sprites automatically show up for icon descriptions
            SpriteAsset.RegisterSpriteAsset();
            base.Load();
            SpirefrostStrings.CreateLocalizedStrings();
            GameMode gameMode = TryGet<GameMode>("GameModeNormal"); //GameModeNormal is the standard game mode. 
            gameMode.classes = gameMode.classes.Append(TryGet<ClassData>(SpireTribe.ID)).ToArray();
            Events.OnEntityCreated += FixImage;
            Events.PostBattle += CleanUpBattleEnd;
            Events.OnBackToMainMenu += CleanUpTemp;

            managedObjects = new GameObject(Title+".ManagedObjects");
            UnityEngine.Object.DontDestroyOnLoad(managedObjects);
            managedObjects.AddComponent<UpdateManager>();

            tempObjects = new GameObject(Title + ".TempObjects");
            UnityEngine.Object.DontDestroyOnLoad(tempObjects);
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
            Events.PostBattle -= CleanUpBattleEnd;
            Events.OnBackToMainMenu -= CleanUpTemp;
            managedObjects.Destroy();
            managedObjects = null;
            tempObjects.Destroy();
            tempObjects = null;
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
        }

        internal T[] RemoveNulls<T>(T[] data) where T : DataFile
        {
            List<T> list = data.ToList();
            list.RemoveAll(x => x == null || x.ModAdded == this);
            return list.ToArray();
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
