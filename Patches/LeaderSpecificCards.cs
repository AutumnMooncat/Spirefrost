using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WildfrostHopeMod.Utils; // Creates TMP_SpriteAsset
using Spirefrost.Builders.Cards.Leaders;
using static Spirefrost.MainModFile;


namespace Spirefrost.Patches
{
    [HarmonyPatch(typeof(CharacterRewards), "Populate")]
    internal static class LeaderSpecificCards
    {
        private static string PoolName(PoolListType type)
        {
            switch (type)
            {
                case PoolListType.IroncladItems:
                case PoolListType.SilentItems:
                case PoolListType.DefectItems:
                case PoolListType.WatcherItems:
                    return "Items";

                case PoolListType.IroncladCharms:
                case PoolListType.SilentCharms:
                case PoolListType.DefectCharms:
                case PoolListType.WatcherCharms:
                    return "Charms";

                case PoolListType.IroncladUnits:
                case PoolListType.SilentUnits:
                case PoolListType.DefectUnits:
                case PoolListType.WatcherUnits:
                    return "Units";
            }

            throw new Exception($"Attempting to create reward pool of non character specific reward type {type}");
        }

        private static RewardPool PoolToReward(PoolListType type)
        {
            RewardPool pool = ScriptableObject.CreateInstance<RewardPool>();
            pool.name = type.ToString();
            pool.type = PoolName(type);
            if (pool.type == "Items" || pool.type == "Units")
            {
                pool.list = PoolToIDs(type).Select(s => MainModFile.instance.TryGet<CardData>(s)).Cast<DataFile>().ToList();
            }
            else if (pool.type == "Charms")
            {
                pool.list = PoolToIDs(type).Select(s => MainModFile.instance.TryGet<CardUpgradeData>(s)).Cast<DataFile>().ToList();
            }
            return pool;
        }

        static void Postfix(CharacterRewards __instance, ClassData classData)
        {
            List<CardData> extraStarters = new List<CardData>();

            if (References.LeaderData.name == Ironclad.FullID)
            {
                __instance.Add(PoolToReward(PoolListType.IroncladItems));
                __instance.Add(PoolToReward(PoolListType.IroncladUnits));
                __instance.Add(PoolToReward(PoolListType.IroncladCharms));
                extraStarters.AddRange(
                    PoolToIDs(PoolListType.IroncladStarterItems).Select(s => MainModFile.instance.TryGet<CardData>(s)).ToArray());
            }
            else if (References.LeaderData.name == Silent.FullID)
            {
                __instance.Add(PoolToReward(PoolListType.SilentItems));
                __instance.Add(PoolToReward(PoolListType.SilentUnits));
                __instance.Add(PoolToReward(PoolListType.SilentCharms));
                extraStarters.AddRange(
                    PoolToIDs(PoolListType.SilentStarterItems).Select(s => MainModFile.instance.TryGet<CardData>(s)).ToArray());
            }
            else if (References.LeaderData.name == Defect.FullID)
            {
                __instance.Add(PoolToReward(PoolListType.DefectItems));
                __instance.Add(PoolToReward(PoolListType.DefectUnits));
                __instance.Add(PoolToReward(PoolListType.DefectCharms));
                extraStarters.AddRange(
                    PoolToIDs(PoolListType.DefectStarterItems).Select(s => MainModFile.instance.TryGet<CardData>(s)).ToArray());
            }
            else if (References.LeaderData.name == Watcher.FullID)
            {
                __instance.Add(PoolToReward(PoolListType.WatcherItems));
                __instance.Add(PoolToReward(PoolListType.WatcherUnits));
                __instance.Add(PoolToReward(PoolListType.WatcherCharms));
                extraStarters.AddRange(
                    PoolToIDs(PoolListType.WatcherStarterItems).Select(s => MainModFile.instance.TryGet<CardData>(s)).ToArray());
            }

            
            //Debug.Log($"LeaderSpecificCards - {References.LeaderData.name} has {extraStarters.Count} additional cards");
            foreach (CardData item in extraStarters)
            {
                References.PlayerData.inventory.deck.Add(item.Clone());
            }
        }
    }
}
