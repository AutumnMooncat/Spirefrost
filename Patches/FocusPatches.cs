using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Spirefrost.Patches
{
    [HarmonyPatch(typeof(StatusEffectData), "RemoveStacks")]
    internal static class RemoveStacksPatch
    {
        static void Prefix(StatusEffectData __instance, ref int amount, bool removeTemporary)
        {
            SpirefrostEvents.InvokePreStatusReduction(__instance, ref amount, removeTemporary);
        }
    }

    // IEnumerator, but we only want Pre and Post so it should be fine?
    [HarmonyPatch]
    internal static class StatusSystemPatch
    {
        internal static bool isTemp;
        internal static Type foundType;

        static MethodBase TargetMethod()
        {
            return SpirefrostUtils.FindEnumeratorMethod(AccessTools.Method(typeof(StatusEffectSystem), nameof(StatusEffectSystem.Apply)), ref foundType);
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();
            FieldInfo isTemp = AccessTools.Field(typeof(StatusSystemPatch), nameof(StatusSystemPatch.isTemp));
            FieldInfo temp = AccessTools.Field(foundType, "temporary");
            bool setInserted = false;
            for (int i = 0; i < codes.Count; i++)
            {
                if (i == codes.Count - 1)
                {
                    Debug.Log("StatusSystemPatch - inserting postfix reset");
                    // false already on stack, consume it and put one back
                    yield return new CodeInstruction(OpCodes.Stsfld, isTemp);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    
                }
                yield return codes[i];
                if (!setInserted && codes[i].opcode == OpCodes.Stloc_1)
                {
                    Debug.Log("StatusSystemPatch - match found, inserting temp check");
                    setInserted = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, temp);
                    yield return new CodeInstruction(OpCodes.Stsfld, isTemp);
                }
            }
        }
    }
}
