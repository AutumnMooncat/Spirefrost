using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Spirefrost.Patches
{
    [HarmonyPatch]
    internal class CleansePatch
    {
        static Type foundType;

        static MethodBase TargetMethod()
        {
            return SpirefrostUtils.FindEnumeratorMethod(AccessTools.Method(typeof(StatusEffectInstantCleanse), nameof(StatusEffectInstantCleanse.Process)), ref foundType);
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();
            MethodInfo negStatus = AccessTools.Method(typeof(StatusEffectData), nameof(StatusEffectData.IsNegativeStatusEffect));
            FieldInfo count = AccessTools.Field(typeof(StatusEffectData), nameof(StatusEffectData.count));
            FieldInfo temp = AccessTools.Field(typeof(StatusEffectData), nameof(StatusEffectData.temporary));
            bool checkInserted = false;
            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (!checkInserted && (codes[i].opcode == OpCodes.Brfalse || codes[i].opcode == OpCodes.Brfalse_S) && i - 1 >= 0 && codes[i - 1].opcode == OpCodes.Callvirt && codes[i - 1].operand as MethodInfo == negStatus)
                {
                    Debug.Log($"CleansePatch - match found, inserting check");
                    checkInserted = false;
                    yield return new CodeInstruction(OpCodes.Ldloc_3); // statusEffectData
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldfld, temp);
                    yield return new CodeInstruction(OpCodes.Stfld, count);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldfld, count);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Bgt, codes[i].operand);
                }
            }
        }
    }
}
