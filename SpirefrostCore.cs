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
using System.IO;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;


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
            gameMode.classes = gameMode.classes.Append(TryGet<ClassData>("Spire")).ToArray();
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
    }

    [HarmonyPatch(typeof(Discarder), "ClearStatusEffects")]
    internal static class DiscarderPatch
    {
        static void Prefix(Discarder __instance, Entity entity)
        {
            SpirefrostEvents.InvokeMovedByDiscarder(entity);
        }
    }

    [HarmonyPatch(typeof(CardPocketSequence), "Move")]
    internal static class DiscoveryPatches
    {
        internal static bool instantSnap;

        static bool Prefix(CardPocketSequence __instance, Entity entity, bool includeRandomness)
        {
            if (instantSnap)
            {
                if (entity.transform.parent == __instance.container.holder)
                {
                    Vector3 childPosition = __instance.container.GetChildPosition(entity);
                    Vector3 childRotation = __instance.container.GetChildRotation(entity);
                    Vector3 childScale = __instance.container.GetChildScale(entity);
                    entity.transform.localPosition = childPosition;
                    entity.transform.localEulerAngles = childRotation;
                    entity.transform.localScale = childScale;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(UpgradeDisplay), "SetData")]
    internal static class CharmOverlay
    {
        static Image CreateAndAlignOverlay(Image other)
        {
            GameObject imageHolder = new GameObject("Charm Overlay");
            Transform imageTransform = imageHolder.transform;
            Transform charmTransform = other.gameObject.transform;
            imageTransform.SetParent(charmTransform, false);
            imageTransform.Align(charmTransform);

            Image overlayImage = imageHolder.AddComponent<Image>();
            RectTransform overlayRect = (RectTransform)overlayImage.transform;
            RectTransform charmImageRect = (RectTransform)other.gameObject.transform;
            overlayRect.Align(charmImageRect);

            overlayImage.material = other.material;
            overlayImage.preserveAspect = other.preserveAspect;
            overlayImage.raycastTarget = other.raycastTarget;

            return overlayImage;
        }

        static void Postfix(UpgradeDisplay __instance, CardUpgradeData data)
        {
            if (data.name.Equals("autumnmooncat.wildfrost.spirefrost.EntropicBrewCharm")) {
                Image overlayImage = CreateAndAlignOverlay(__instance.image);
                overlayImage.sprite = MainModFile.instance.entropicOverlay.ToSprite();
                __instance.image.sprite = MainModFile.instance.entropicUnderlay.ToSprite();
            } 
            else if (data.name.Equals("autumnmooncat.wildfrost.spirefrost.DuplicationCharm"))
            {
                Image overlayImage = CreateAndAlignOverlay(__instance.image);
                overlayImage.sprite = MainModFile.instance.duplicationOverlay.ToSprite();
                __instance.image.sprite = MainModFile.instance.duplicationUnderlay.ToSprite();
            }
        }
    }

    [HarmonyPatch(typeof(CardCharm), "Update")]
    internal static class RainbowCharms
    {
        static readonly List<String> rainbowEnabled = new List<String>()
        {
            "autumnmooncat.wildfrost.spirefrost.EntropicBrewCharm",
            "autumnmooncat.wildfrost.spirefrost.DuplicationCharm"
        };

        static float ToRadians(float degrees)
        {
            return degrees * (float)Math.PI / 180;
        }

        static void Postfix(CardCharm __instance)
        {
            if (rainbowEnabled.Contains(__instance.data.name))
            {
                if (!MainModFile.instance.updated)
                {
                    float r = (float)((Math.Cos(ToRadians((Environment.TickCount + 0000L) / 10L % 360L)) + 1.25F) / 2.3F);
                    float g = (float)((Math.Cos(ToRadians((Environment.TickCount + 1000L) / 10L % 360L)) + 1.25F) / 2.3F);
                    float b = (float)((Math.Cos(ToRadians((Environment.TickCount + 2000L) / 10L % 360L)) + 1.25F) / 2.3F);
                    MainModFile.instance.rainbowColor = new Color(r, g, b, 1.0f);
                    MainModFile.instance.updated = true;
                }
                __instance.image.color = MainModFile.instance.rainbowColor;
            }
        }
    }

    [HarmonyPatch(typeof(References), nameof(References.Classes), MethodType.Getter)]
    internal static class FixClassesGetter
    {
        static void Postfix(ref ClassData[] __result) => __result = AddressableLoader.GetGroup<ClassData>("ClassData").ToArray();
    }

    [HarmonyPatch(typeof(PetHutFlagSetter), "SetupFlag")]
    internal static class PatchInPetFlag
    {
        static Sprite louseFlag;
        static void Postfix(PetHutFlagSetter __instance)
        {
            if (louseFlag == null)
            {
                Texture2D louseTex = MainModFile.instance.ImagePath("LouseFlag.png").ToTex();
                louseFlag = Sprite.Create(louseTex, new Rect(0f, 0f, louseTex.width, louseTex.height), new Vector2(0.5f, 1.0f), 160);
            }

            int petIndex = SaveSystem.LoadProgressData("selectedPet", 0);
            string[] petInfo = MetaprogressionSystem.GetUnlockedPets();

            if (petIndex < petInfo.Length && petInfo[petIndex].Equals(Extensions.PrefixGUID("louse", MainModFile.instance)))
            {
                __instance.flag.sprite = louseFlag;
            }
        }
    }

    [HarmonyPatch(typeof(TribeHutSequence), "SetupFlags")]
    internal static class PatchTribeHut
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
            gameObject2.transform.GetChild(3).GetChild(0).GetComponent<LocalizeStringEvent>().StringReference = collection.GetString(SpirefrostStrings.TribeDescKey);

            //4:Title Ribbon (Image)
            //4-0: Text (LocalizedString)
            gameObject2.transform.GetChild(4).GetChild(0).GetComponent<LocalizeStringEvent>().StringReference = collection.GetString(SpirefrostStrings.TribeTitleKey);
        }
    }
}
