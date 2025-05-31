using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using Spirefrost.Patches;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WildfrostHopeMod.Utils;
using WildfrostHopeMod.VFX;
using static Spirefrost.Patches.CustomStatusIconPatches;

namespace Spirefrost
{
    internal static class CardDataExtensions
    {
        internal static void GiveUpgrade(this CardData target, string name = "Crown") // Give a crown by default
        {
            MainModFile.instance.TryGet<CardUpgradeData>(name).Clone().Assign(target);
        }

        internal static void SetRandomHealth(this CardData target, int min, int max)
        {
            target.hp = new Vector2Int(min, max).Random();
            target.hp = Mathf.Max(1, target.hp);
            target.hasHealth = true;
        }

        internal static void SetRandomDamage(this CardData target, int min, int max)
        {
            target.damage = new Vector2Int(min, max).Random();
            target.damage = Mathf.Max(0, target.damage);
            target.hasAttack = true;
        }

        internal static void SetRandomCounter(this CardData target, int min, int max)
        {
            target.counter = new Vector2Int(min, max).Random();
            target.counter = Mathf.Max(1, target.counter);
        }

        internal static void SetRandomPassive(this CardData target, string passiveEffect, int min, int max)
        {
            StatusEffectData effect = MainModFile.instance.TryGet<StatusEffectData>(passiveEffect);
            target.startWithEffects = target.startWithEffects.With(new CardData.StatusEffectStacks(effect, new Vector2Int(min, max).Random()));
        }

        internal static void SetRandomActive(this CardData target, string attackEffect, int min, int max)
        {
            StatusEffectData effect = MainModFile.instance.TryGet<StatusEffectData>(attackEffect);
            target.attackEffects = target.attackEffects.With(new CardData.StatusEffectStacks(effect, new Vector2Int(min, max).Random()));
        }

        internal static void SetRandomTrait(this CardData target, string traitName, int min, int max)
        {
            TraitData trait = MainModFile.instance.TryGet<TraitData>(traitName);
            if (target.traits == null)
            {
                target.traits = new List<CardData.TraitStacks>();
            }
            target.traits.Add(new CardData.TraitStacks(trait, new Vector2Int(min, max).Random()));
        }

        internal static void SetWide(this CardData data, bool wide = true)
        {
            data.SetCustomData(WideUtils.WideKey, wide);
        }

        internal static bool IsWide(this CardData data)
        {
            data.TryGetCustomData(WideUtils.WideKey, out bool ret, false);
            return ret;
        }
    }

    internal static class CharacterExtensions
    {
        internal static int TotalOpenSlots(this Character owner)
        {
            int free = 0;
            foreach (var item in Battle.instance.GetRows(owner))
            {
                if (item is CardSlotLane lane)
                {
                    foreach (var slot in lane.slots)
                    {
                        if (slot.Count < slot.max)
                        {
                            free++;
                        }
                    }
                }
            }
            return free;
        }
    }

    internal static class EntityExtensions
    {
        internal static CardSlot[] GetContainingSlots(this Entity entity)
        {
            return entity.actualContainers.Select(cont => cont as CardSlot).Where(slot => slot != null).ToArray();
        }
    }

    internal static class RectTransformExtensions
    {
        internal static void ScaleOffsets(this RectTransform rectTransform, Vector2 scale)
        {
            rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x * scale.x, rectTransform.offsetMin.y * scale.y);
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x * scale.x, rectTransform.offsetMax.y * scale.y);
        }

        internal static void TranslateViaSet(this RectTransform rectTransform, Vector3 offset)
        {
            rectTransform.SetLocalPositionAndRotation(new Vector3(rectTransform.localPosition.x + offset.x, rectTransform.localPosition.y + offset.y, rectTransform.localPosition.z + offset.y), rectTransform.localRotation);
        }
    }

    internal static class CardContainerExtensions
    {
        internal static bool HasSlotBehind(this CardSlot slot)
        {
            CardSlotLane lane = slot.GetContainingLane();
            if (lane != null)
            {
                if (lane.slots.IndexOf(slot) + 1 < lane.slots.Count)
                {
                    return true;
                }
            }
            return false;
        }

        internal static CardSlot GetSlotBehind(this CardSlot slot)
        {
            CardSlotLane lane = slot.GetContainingLane();
            if (lane != null)
            {
                int i = lane.slots.IndexOf(slot) + 1;
                if (i < lane.slots.Count)
                {
                    return lane.slots[i];
                }
            }
            return null;
        }

        internal static CardSlot GetSlotInFront(this CardSlot slot)
        {
            CardSlotLane lane = slot.GetContainingLane();
            if (lane != null)
            {
                int i = lane.slots.IndexOf(slot) - 1;
                if (i >= 0)
                {
                    return lane.slots[i];
                }
            }
            return null;
        }

        internal static int GetXCoord(this CardSlot slot)
        {
            List<CardSlot> slots = slot.GetContainingLane()?.slots;
            return slots?.IndexOf(slot) ?? -1;
        }

        internal static int GetYCoord(this CardSlot slot)
        {
            return Battle.instance.GetRowIndex(slot.GetContainingLane());
        }

        internal static CardSlot GetRelativeSlot(this CardSlot slot, int dx, int dy)
        {
            List<CardContainer> rows = Battle.instance.GetRows(slot.owner);
            int rowIndex = slot.GetYCoord() + dy;
            int slotIndex = slot.GetXCoord() + dx;
            if (rowIndex >= 0 && rowIndex < Battle.instance.rowCount)
            {
                if (rows[rowIndex] is CardSlotLane lane)
                {
                    if (slotIndex >= 0 && slotIndex < lane.slots.Count)
                    {
                        return lane.slots[slotIndex];
                    }
                }
            }
            return null;
        }

        internal static CardSlotLane GetContainingLane(this CardSlot slot)
        {
            return slot.Group as CardSlotLane;
        }
    }

    internal static class Texture2DExtensions
    {
        internal static void OverlayTextures(this Texture2D mainTex, Texture2D overlay, Texture2D underlay, Color underlayColor)
        {
            Color[] overlayPixels = overlay.GetPixels();
            Color[] underlayPixels = underlay.GetPixels();
            Color[] mainPixels = mainTex.GetPixels();
            for (int i = 0; i < mainPixels.Length; i++)
            {
                Color underlayPixel = underlayPixels[i];
                Color overlayPixel = overlayPixels[i];
                bool hasUnderlay = underlayPixel.a > 0;
                bool hasOverlay = overlayPixel.a > 0;
                if (hasUnderlay)
                {
                    float lightness = (underlayPixel.r + underlayPixel.g + underlayPixel.b) / 3;
                    underlayPixel.r = lightness * underlayColor.r;
                    underlayPixel.g = lightness * underlayColor.g;
                    underlayPixel.b = lightness * underlayColor.b;

                    //Blend Both
                    if (hasOverlay)
                    {
                        float src = overlayPixel.a;
                        float dst = 1f - src;
                        float alpha = src + (dst * underlayPixel.a);
                        Color result = (overlayPixel * src + underlayPixel * underlayPixel.a * dst) / alpha;
                        result.a = alpha;
                        mainPixels[i] = result;
                    }
                    //Just Underlay
                    else
                    {
                        mainPixels[i] = underlayPixel;
                    }
                }
                else
                {
                    //Just Overlay
                    if (hasOverlay)
                    {
                        mainPixels[i] = overlayPixel;
                    }
                    //Empty
                    else
                    {
                        mainPixels[i].a = 0f;
                    }
                }  
            }
            mainTex.SetPixels(mainPixels);
            mainTex.Apply();
        }
    }

    internal static class TransformExtensions
    {
        internal static void Align(this Transform self, Transform target)
        {
            self.localPosition = target.localPosition;
            self.localEulerAngles = target.localEulerAngles;
            self.localScale = target.localScale;
        }

        internal static void Align(this RectTransform self, RectTransform target)
        {
            self.localPosition = target.localPosition;
            self.localEulerAngles = target.localEulerAngles;
            self.localScale = target.localScale;
            self.anchoredPosition = target.anchoredPosition;
            self.anchoredPosition3D = target.anchoredPosition3D;
            self.anchorMax = target.anchorMax;
            self.anchorMin = target.anchorMin;
            self.offsetMax = target.offsetMax;
            self.offsetMin = target.offsetMin;
            self.pivot = target.pivot;
            self.sizeDelta = target.sizeDelta;
        }
    }

    internal static class StringExtensions
    {
        internal static Texture2D ToNamedTex(this String self)
        {
            Texture2D tex = new Texture2D(0, 0, TextureFormat.RGBA32, mipChain: false)
            {
                name = Path.GetFileNameWithoutExtension(MainModFile.instance.ImagePath(self)) + "Texture2D"
            };
            tex.LoadImage(File.ReadAllBytes(MainModFile.instance.ImagePath(self)));
            return tex;
        }
        internal static Sprite ToNamedSprite(this String self)
        {
            Sprite sprite = ToNamedTex(self).ToSprite();
            sprite.name = Path.GetFileNameWithoutExtension(MainModFile.instance.ImagePath(self)) + "Sprite";
            return sprite;
        }
    }

    internal static class StatusIconExtensions
    {
        internal static void LinkStatus(this StatusIcon icon, StatusEffectData toLink)
        {
            SpirefrostUtils.SetNamedReference(icon, HasLinkedKey, true);
            SpirefrostUtils.SetNamedReference(icon, LinkedKey, toLink);
        }

        internal static bool HasLinkedStatus(this StatusIcon icon)
        {
            return SpirefrostUtils.GetNamedReference(icon, HasLinkedKey) is bool check && check;
        }

        internal static StatusEffectData GetLinkedStatus(this StatusIcon icon)
        {
            return SpirefrostUtils.GetNamedReference(icon, LinkedKey) as StatusEffectData;
        }
    }
}
