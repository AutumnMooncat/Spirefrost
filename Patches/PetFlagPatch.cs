using Deadpan.Enums.Engine.Components.Modding;
using HarmonyLib;
using UnityEngine;
using Extensions = Deadpan.Enums.Engine.Components.Modding.Extensions;


namespace Spirefrost.Patches
{
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
}
