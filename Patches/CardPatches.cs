using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Spirefrost.Patches
{
    [HarmonyPatch]
    internal class CardUpdateDataPatch
    {
        internal static bool shortCircuitOnce = false;

        internal static bool CheckShortCircuit()
        {
            if (shortCircuitOnce)
            {
                shortCircuitOnce = false;
                return true;
            }
            return false;
        }

        static MethodBase TargetMethod()
        {
            return AccessTools.Method(Type.GetType("Card+<UpdateData>d__32,Assembly-CSharp"), "MoveNext");
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            Label jumpLabel = generator.DefineLabel();
            MethodInfo checkMethod = AccessTools.Method(typeof(CardUpdateDataPatch), nameof(CheckShortCircuit));
            MethodInfo setDescMethod = AccessTools.Method(typeof(Card), nameof(Card.SetDescription));
            bool checkInserted = false;
            bool jumpInserted = false;
            for (int i = 0; i < codes.Count; i++)
            {
                if (!checkInserted && codes[i].opcode == OpCodes.Ldloc_2 && i + 4 < codes.Count)
                {
                    if (codes[i + 1].opcode == OpCodes.Ldloc_2 && codes[i + 2].opcode == OpCodes.Ldfld && codes[i + 3].opcode == OpCodes.Callvirt && codes[i + 4].opcode == OpCodes.Callvirt)
                    {
                        Debug.Log("CardUpdateDataPatch - Match found, injecting check");
                        checkInserted = true;
                        yield return new CodeInstruction(OpCodes.Call, checkMethod);
                        yield return new CodeInstruction(OpCodes.Brtrue, jumpLabel);
                    }
                }
                if (!jumpInserted && codes[i].opcode == OpCodes.Ldloc_2 && i + 1 < codes.Count)
                {
                    if (codes[i + 1].opcode == OpCodes.Call && codes[i + 1].operand is MethodInfo info && info == setDescMethod)
                    {
                        Debug.Log("CardUpdateDataPatch - Match found, adding jump label");
                        jumpInserted = true;
                        codes[i].labels.Add(jumpLabel);
                    }
                }
                yield return codes[i];
            }
        }
    }
}
