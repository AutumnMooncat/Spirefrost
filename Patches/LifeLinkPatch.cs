using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spirefrost.Patches
{
    [HarmonyPatch(typeof(EntityDisplay), "UpdateData")]
    internal class LifeLinkPatch
    {
        internal static readonly Dictionary<Entity, float> tracked = new Dictionary<Entity, float>();
        static void Prefix(EntityDisplay __instance)
        {
            if (tracked.ContainsKey(__instance.entity))
            {
                float factor = tracked[__instance.entity];
                int newHealth = factor >= 1f ? Mathf.CeilToInt(__instance.entity.hp.max * factor) : Mathf.FloorToInt(__instance.entity.hp.max * factor);
                __instance.entity.hp.current = Math.Max(1, newHealth);
                __instance.entity.hp.max = Math.Max(__instance.entity.hp.max, newHealth);
                tracked.Remove(__instance.entity);
            }
        }
    }
}
