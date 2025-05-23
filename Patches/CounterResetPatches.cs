using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Spirefrost.Patches
{
    internal class CounterResetLogic
    {
        static bool ProcessEntity(Entity entity)
        {
            SpirefrostEvents.InvokeCounterReset(entity);
            // Hopefully this works
            IEnumerator pause = Sequences.Wait(0.167f);
            while (pause.MoveNext()) { }

            // Return if we need to process again
            if (entity.counter.current <= 0)
            {
                return true;
            }
            return false;
        }
    }

    [HarmonyPatch]
    internal static class CheckUnitsPatch
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.Method(Type.GetType("Battle+<CheckUnitsTakeTurns>d__73,Assembly-CSharp"), "MoveNext");
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            FieldInfo cancelTurn = AccessTools.Field(typeof(Battle), "cancelTurn");
            MethodInfo promptUpdate = AccessTools.Method(typeof(Entity), "PromptUpdate");

            Label jumpBackLabel = generator.DefineLabel();
            CodeInstruction jumpBack = new CodeInstruction(OpCodes.Nop);
            jumpBack.labels.Add(jumpBackLabel);
            bool jumpInserted = false;

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldloc_2 && i + 1 < codes.Count && codes[i + 1].opcode == OpCodes.Ldfld)
                {
                    if (codes[i + 1].operand is FieldInfo info && info == cancelTurn && !jumpInserted)
                    {
                        MainModFile.Print("CheckUnitsPatch - Match found, injecting new jump");
                        jumpInserted = true;
                        yield return jumpBack;
                    }
                }
                if (codes[i].opcode == OpCodes.Callvirt)
                {
                    if (codes[i].operand is MethodInfo info && info == promptUpdate && jumpInserted)
                    {
                        MainModFile.Print("CheckUnitsPatch - Match found, injecting new instructions");
                        // Ldarg0 and Ldfld already put Entity on the stack
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CounterResetLogic), "ProcessEntity"));
                        yield return new CodeInstruction(OpCodes.Brtrue, jumpBackLabel);
                        // Put Entity back on the stack
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, codes[i - 1].operand);
                    }
                }
                yield return codes[i];
            }
        }
    }

    [HarmonyPatch]
    internal static class ProcessUnitPatch
    {
        private static Type createdType;
        static MethodBase TargetMethod()
        {
            return SpirefrostUtils.FindEnumeratorMethod(AccessTools.DeclaredMethod(typeof(Battle), "ProcessUnit", new Type[] { typeof(Entity) }), ref createdType);
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            FieldInfo counter = AccessTools.Field(typeof(Entity), "counter");
            MethodInfo countDown = AccessTools.Method(typeof(Battle), "CardCountDown");
            MethodInfo promptUpdate = AccessTools.Method(typeof(Entity), "PromptUpdate");

            Label jumpBackLabel = generator.DefineLabel();
            CodeInstruction jumpBack = new CodeInstruction(OpCodes.Nop);
            jumpBack.labels.Add(jumpBackLabel);
            bool jumpInserted = false;
            bool countDownFound = false;

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call)
                {
                    if (codes[i].operand is MethodInfo info && info == countDown)
                    {
                        countDownFound = true;
                    }
                }
                if (countDownFound && codes[i].opcode == OpCodes.Ldarg_0 && i + 2 < codes.Count && codes[i + 1].opcode == OpCodes.Ldfld && codes[i + 2].opcode == OpCodes.Ldflda)
                {
                    if (codes[i + 2].operand is FieldInfo info && info == counter && !jumpInserted)
                    {
                        Debug.Log("ProcessUnitPatch - Match found, injecting new jump");
                        jumpInserted = true;
                        countDownFound = false;
                        yield return jumpBack;
                    }
                }
                if (codes[i].opcode == OpCodes.Callvirt)
                {
                    if (codes[i].operand is MethodInfo info && info == promptUpdate && jumpInserted)
                    {
                        Debug.Log("ProcessUnitPatch - Match found, injecting new instructions");
                        // Ldarg0 and Ldfld already put Entity on the stack
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CounterResetLogic), "ProcessEntity"));
                        yield return new CodeInstruction(OpCodes.Brtrue, jumpBackLabel);
                        // Put Entity back on the stack
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, codes[i - 1].operand);
                    }
                }
                yield return codes[i];
            }
        }
    }
}
