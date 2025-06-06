using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spirefrost.Patches
{
    [HarmonyPatch]
    internal class TemporaryCharmPatches
    {
        internal static void OnLoad(Entity entity, string charmID)
        {
            CardData mirror = entity.GetOrMakeMirroredData();
            CardUpgradeData charm = MainModFile.instance.TryGet<CardUpgradeData>(charmID).Clone();
            charm.Assign(mirror);
            charm.Display(entity);
        }

        [HarmonyPatch]
        internal static class InspectSystemPatches
        {
            [HarmonyPatch(typeof(InspectSystem), nameof(InspectSystem.Inspect))]
            internal static class InspectPatch
            {
                static void Postfix(InspectSystem __instance)
                {
                    CardData mirror = __instance.inspect.GetMirroredData();
                    if (mirror)
                    {
                        __instance.hasAnyCharms = mirror.upgrades.Any(data => data.type == CardUpgradeData.Type.Charm);
                        __instance.inspectCharmsLayout.SetActive(__instance.hasAnyCharms);
                    }
                }
            }
        }

        [HarmonyPatch]
        internal static class InspectCharmsSystemPatch
        {
            [HarmonyPatch(typeof(InspectCharmsSystem), nameof(InspectCharmsSystem.Create))]
            internal static class CreatePatch
            {
                static void Prefix(InspectCharmsSystem __instance, ref CardUpgradeData[] cardUpgrades)
                {
                    CardData mirror = __instance.inspectSystem.inspect.GetMirroredData();
                    if (mirror)
                    {
                        cardUpgrades = mirror.upgrades.Where(data => data.type == CardUpgradeData.Type.Charm).ToArray();
                    }
                }
            }
        }
    }
}
