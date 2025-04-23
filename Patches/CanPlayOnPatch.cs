using Dead;
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
    [HarmonyPatch(typeof(Entity), "CanPlayOn", typeof(Entity), typeof(bool))]
    internal class CanPlayOnPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo getMax = AccessTools.Method(typeof(Stat), "get_max");

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call)
                {
                    if (codes[i].operand is MethodInfo info && info == getMax)
                    {
                        Debug.Log("CanPlayOnPatch - Match found, injecting new instructions");
                        // Ldflda already put Entity::damage on the stack
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Stat), "get_current"));
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(Entity), "tempDamage"));
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SafeInt), "get_Value"));
                        yield return new CodeInstruction(OpCodes.Add);
                        // Contiue to skip the original instruction
                        continue;
                    }
                }
                yield return codes[i];
            }
        }
    }
}
