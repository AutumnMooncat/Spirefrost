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


namespace Spirefrost
{
    public class MainModFile : WildfrostMod
    {
        public static MainModFile instance;
        private bool preLoaded;

        // This allows for icons in descriptions
        public override TMP_SpriteAsset SpriteAsset => spriteAsset;
        internal static TMP_SpriteAsset spriteAsset;

        private Color rainbowColor = new Color(1f, 1f, 1f, 1f);

        internal Texture2D overlay;
        internal Texture2D underlay;

        public MainModFile(string modDirectory) : base(modDirectory)
        { 
            instance = this;
            /***
             * CardCharm.image.color is promising
            /*this.liquidColor.r = (MathUtils.cosDeg((float)(System.currentTimeMillis() / 10L % 360L)) + 1.25F) / 2.3F;// 674
            this.liquidColor.g = (MathUtils.cosDeg((float)((System.currentTimeMillis() + 1000L) / 10L % 360L)) + 1.25F) / 2.3F;// 675
            this.liquidColor.b = (MathUtils.cosDeg((float)((System.currentTimeMillis() + 2000L) / 10L % 360L)) + 1.25F) / 2.3F;// 676
            */
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
                overlay = new Texture2D(0, 0, TextureFormat.RGBA32, mipChain: false)
                {
                    name = Path.GetFileNameWithoutExtension(ImagePath("Charms/EntropicCharmOverlay.png"))
                };
                overlay.LoadImage(File.ReadAllBytes(ImagePath("Charms/EntropicCharmOverlay.png")));
                underlay = new Texture2D(0, 0, TextureFormat.RGBA32, mipChain: false)
                {
                    name = Path.GetFileNameWithoutExtension(ImagePath("Charms/EntropicCharmUnderlay.png"))
                };
                underlay.LoadImage(File.ReadAllBytes(ImagePath("Charms/EntropicCharmUnderlay.png")));
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
                "Well versed in defending themselves, they whittle their enemies down to win the war of attrition.");                                  //Create the description.

        }
    }

    [HarmonyPatch(typeof(CardCharm), "Update")]
    internal static class TrackImages
    {
        static float ToRadians(float degrees)
        {
            return degrees * (float)Math.PI / 180;
        }

        static void Postfix(CardCharm __instance)
        {
            /*if (__instance.data.name.Equals("autumnmooncat.wildfrost.spirefrost.EntropicBrewCharm"))
            {
                float r = (float)((Math.Cos(ToRadians((Environment.TickCount + 0000L) / 10L % 360L)) + 1.25F) / 2.3F);
                float g = (float)((Math.Cos(ToRadians((Environment.TickCount + 1000L) / 10L % 360L)) + 1.25F) / 2.3F);
                float b = (float)((Math.Cos(ToRadians((Environment.TickCount + 2000L) / 10L % 360L)) + 1.25F) / 2.3F);

                Color current = __instance.image.color;
                current.r = r;
                current.g = g;
                current.b = b;
                __instance.image.color = current;
            }*/
            if (__instance.data.name.Equals("autumnmooncat.wildfrost.spirefrost.EntropicBrewCharm"))
            {
                float r = (float)((Math.Cos(ToRadians((Environment.TickCount + 0000L) / 10L % 360L)) + 1.25F) / 2.3F);
                float g = (float)((Math.Cos(ToRadians((Environment.TickCount + 1000L) / 10L % 360L)) + 1.25F) / 2.3F);
                float b = (float)((Math.Cos(ToRadians((Environment.TickCount + 2000L) / 10L % 360L)) + 1.25F) / 2.3F);

                Color rainbow = new Color(r, g, b, 1.0f);
                __instance.image.sprite.texture.OverlayTextures(MainModFile.instance.overlay, MainModFile.instance.underlay, rainbow);
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
            gameObject2.transform.GetChild(3).GetChild(0).GetComponent<LocalizeStringEvent>().StringReference = collection.GetString(MainModFile.instance.TribeDescKey);

            //4:Title Ribbon (Image)
            //4-0: Text (LocalizedString)
            gameObject2.transform.GetChild(4).GetChild(0).GetComponent<LocalizeStringEvent>().StringReference = collection.GetString(MainModFile.instance.TribeTitleKey);
        }
    }
}
