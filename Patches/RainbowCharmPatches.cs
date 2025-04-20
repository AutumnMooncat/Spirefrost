using Deadpan.Enums.Engine.Components.Modding;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Spirefrost.Builders.CardUpgrades;
using System.Collections.Generic;
using System;


namespace Spirefrost.Patches
{
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
            if (data.name.Equals(EntropicPotion.FullID)) {
                Image overlayImage = CreateAndAlignOverlay(__instance.image);
                overlayImage.sprite = MainModFile.instance.entropicOverlay.ToSprite();
                __instance.image.sprite = MainModFile.instance.entropicUnderlay.ToSprite();
            } 
            else if (data.name.Equals(DuplicationPotion.FullID))
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
            EntropicPotion.FullID,
            DuplicationPotion.FullID
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
}
