using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

        internal static void SetRandomActive(this CardData target, string passiveEffect, int min, int max)
        {
            StatusEffectData effect = MainModFile.instance.TryGet<StatusEffectData>(passiveEffect);
            target.attackEffects = target.attackEffects.With(new CardData.StatusEffectStacks(effect, new Vector2Int(min, max).Random()));
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
}
