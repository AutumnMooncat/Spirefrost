using HarmonyLib;
using Spirefrost.StatusEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Spirefrost.Patches
{
    internal class CounterResetLogic
    {
        static bool willNormalTrigger;

        static bool ProcessEntity(Entity entity)
        {
            // Reset an extra counter if its at 0 and we didnt just trigger off the main counter
            StatusEffectExtraCounter toProcess = ExtraCounterPatches.NextCustomCounterAtZero(entity);
            Debug.Log($"Found an extra counter to process? {toProcess != null}");
            Debug.Log($"Did we just trigger off normal counter? {willNormalTrigger}");
            if (toProcess && !willNormalTrigger)
            {
                // Reset the first extra counter and let any others stay at 0 for more triggers
                Debug.Log($"Resetting extra counter {toProcess}");
                toProcess.ResetCounter();
            }

            // Now that a counter has been reset (either the main or an extra), fire the hook for on reset
            SpirefrostEvents.InvokeCounterReset(entity);

            // Hopefully this works
            IEnumerator pause = Sequences.Wait(0.167f);
            while (pause.MoveNext()) { }

            // Return if we need to process again
            toProcess = ExtraCounterPatches.NextCustomCounterAtZero(entity);
            Debug.Log($"Actual counter is ready to trigger? {entity.counter.current <= 0}");
            Debug.Log($"An extra counter is ready to trigger? {toProcess != null}");
            if (entity.counter.current <= 0 || toProcess != null)
            {
                entity.PromptUpdate();
                return true;
            }
            return false;
        }

        static int LowestCounterValue(int current, Entity entity)
        {
            willNormalTrigger = current == 0;
            int ret = current;
            foreach (var item in ExtraCounterPatches.GetCustomCounterIcons(entity))
            {
                ret = Math.Min(ret, item.GetValue().current);
            }
            return ret;
        }
    }

    [HarmonyPatch]
    internal static class CheckUnitsPatch
    {
        static Type enumeratorType = Type.GetType("Battle+<CheckUnitsTakeTurns>d__73,Assembly-CSharp");

        static MethodBase TargetMethod()
        {
            return AccessTools.Method(enumeratorType, "MoveNext");
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            FieldInfo cancelTurn = AccessTools.Field(typeof(Battle), nameof(Battle.cancelTurn));
            FieldInfo unit = AccessTools.Field(enumeratorType, "<unit>5__4");
            FieldInfo counter = AccessTools.Field(typeof(Entity), nameof(Entity.counter));
            MethodInfo promptUpdate = AccessTools.Method(typeof(Entity), nameof(Entity.PromptUpdate));
            MethodInfo process = AccessTools.Method(typeof(CounterResetLogic), "ProcessEntity");
            MethodInfo lowest = AccessTools.Method(typeof(CounterResetLogic), "LowestCounterValue");
            MethodInfo getCurr = AccessTools.Method(typeof(Stat), "get_current");
            MethodInfo getMax = AccessTools.Method(typeof(Stat), "get_max");
            MethodInfo setCurr = AccessTools.Method(typeof(Stat), "set_current");

            Label skipResetLabel = generator.DefineLabel();
            Label jumpBackLabel = generator.DefineLabel();
            CodeInstruction jumpBack = new CodeInstruction(OpCodes.Nop);
            jumpBack.labels.Add(jumpBackLabel);

            bool jumpInserted = false;
            bool processInserted = false;
            bool skipInserted = false;

            for (int i = 0; i < codes.Count; i++)
            {
                if (!jumpInserted && codes[i].opcode == OpCodes.Ldloc_2 && i + 1 < codes.Count && codes[i + 1].opcode == OpCodes.Ldfld)
                {
                    if (codes[i + 1].operand is FieldInfo info && info == cancelTurn)
                    {
                        MainModFile.Print("CheckUnitsPatch - Match found, injecting jump back");
                        jumpInserted = true;
                        yield return jumpBack;
                    }
                }
                if (!processInserted && codes[i].opcode == OpCodes.Callvirt)
                {
                    if (codes[i].operand is MethodInfo info && info == promptUpdate && jumpInserted && i - 2 >= 0)
                    {
                        MainModFile.Print("CheckUnitsPatch - Match found, injecting new instructions and skip jump");
                        processInserted = true;
                        codes[i - 2].labels.Add(skipResetLabel);
                        // Ldarg0 and Ldfld already put Entity on the stack
                        yield return new CodeInstruction(OpCodes.Call, process);
                        yield return new CodeInstruction(OpCodes.Brtrue, jumpBackLabel);
                        // Put Entity back on the stack
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, codes[i - 1].operand);
                    }
                }
                if (!skipInserted && codes[i].opcode == OpCodes.Ldarg_0 && i + 4 < codes.Count)
                {
                    if (codes[i + 3].opcode == OpCodes.Call && codes[i + 3].operand as MethodInfo == getMax && codes[i + 4].opcode == OpCodes.Call && codes[i + 4].operand as MethodInfo == setCurr)
                    {
                        MainModFile.Print("CheckUnitsPatch - current reset found, inserting check");
                        skipInserted = true;
                        // counter already on stack
                        yield return new CodeInstruction(OpCodes.Call, getCurr);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                        yield return new CodeInstruction(OpCodes.Bgt, skipResetLabel);
                        // put counter back
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, unit);
                        yield return new CodeInstruction(OpCodes.Ldflda, counter);
                    }
                }
                yield return codes[i];
                if (codes[i].opcode == OpCodes.Call && codes[i].operand as MethodInfo == getCurr)
                {
                    MainModFile.Print("CheckUnitsPatch - Stat.current found, wrapping");
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, unit);
                    yield return new CodeInstruction(OpCodes.Call, lowest);
                }
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
            FieldInfo unit = AccessTools.Field(createdType, "unit");
            MethodInfo countDown = AccessTools.Method(typeof(Battle), "CardCountDown");
            MethodInfo promptUpdate = AccessTools.Method(typeof(Entity), "PromptUpdate");
            MethodInfo process = AccessTools.Method(typeof(CounterResetLogic), "ProcessEntity");
            MethodInfo lowest = AccessTools.Method(typeof(CounterResetLogic), "LowestCounterValue");
            MethodInfo getCurr = AccessTools.Method(typeof(Stat), "get_current");
            MethodInfo getMax = AccessTools.Method(typeof(Stat), "get_max");
            MethodInfo setCurr = AccessTools.Method(typeof(Stat), "set_current");

            Label skipResetLabel = generator.DefineLabel();
            Label jumpBackLabel = generator.DefineLabel();
            CodeInstruction jumpBack = new CodeInstruction(OpCodes.Nop);
            jumpBack.labels.Add(jumpBackLabel);

            bool jumpInserted = false;
            bool countDownFound = false;
            bool skipInserted = false;

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call)
                {
                    if (codes[i].operand is MethodInfo info && info == countDown)
                    {
                        countDownFound = true;
                    }
                }
                if (!jumpInserted && countDownFound && codes[i].opcode == OpCodes.Ldarg_0 && i + 2 < codes.Count && codes[i + 1].opcode == OpCodes.Ldfld && codes[i + 2].opcode == OpCodes.Ldflda)
                {
                    if (codes[i + 2].operand is FieldInfo info && info == counter)
                    {
                        Debug.Log("ProcessUnitPatch - Match found, injecting new jump");
                        jumpInserted = true;
                        countDownFound = false;
                        yield return jumpBack;
                    }
                }
                if (jumpInserted && codes[i].opcode == OpCodes.Callvirt)
                {
                    if (codes[i].operand is MethodInfo info && info == promptUpdate && i - 2 >= 0)
                    {
                        Debug.Log("ProcessUnitPatch - Match found, injecting new instructions and skip jump");
                        codes[i - 2].labels.Add(skipResetLabel);
                        // Ldarg0 and Ldfld already put Entity on the stack
                        yield return new CodeInstruction(OpCodes.Call, process);
                        yield return new CodeInstruction(OpCodes.Brtrue, jumpBackLabel);
                        // Put Entity back on the stack
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, codes[i - 1].operand);
                    }
                }
                if (!skipInserted && codes[i].opcode == OpCodes.Ldarg_0 && i + 3 < codes.Count)
                {
                    if (codes[i + 3].opcode == OpCodes.Call && codes[i + 3].operand as MethodInfo == getMax && codes[i + 4].opcode == OpCodes.Call && codes[i + 4].operand as MethodInfo == setCurr)
                    {
                        MainModFile.Print("ProcessUnitPatch - current reset found, inserting check");
                        skipInserted = true;
                        // counter already on stack
                        yield return new CodeInstruction(OpCodes.Call, getCurr);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                        yield return new CodeInstruction(OpCodes.Bgt, skipResetLabel);
                        // put counter back
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, unit);
                        yield return new CodeInstruction(OpCodes.Ldflda, counter);
                    }
                }
                yield return codes[i];
                if (codes[i].opcode == OpCodes.Call && codes[i].operand as MethodInfo == getCurr)
                {
                    MainModFile.Print("ProcessUnitPatch - Stat.current found, wrapping");
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, unit);
                    yield return new CodeInstruction(OpCodes.Call, lowest);
                }
            }
        }
    }
}
