using HarmonyLib;
using MonoMod.Utils;
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

        internal static IEnumerator ProcessEntity(Entity entity)
        {
            Debug.Log($"Trigger finished for {entity}");
            foreach (var item in entity.statusEffects)
            {
                if (item is StatusEffectExtraCounter extra)
                {
                    Debug.Log($"Found extra counter: {extra.count}/{extra.maxCount}");
                }
            }
            // Reset an extra counter if its at 0 and we didnt just trigger off the main counter
            StatusEffectExtraCounter toProcess = ExtraCounterPatches.NextCustomCounterAtZero(entity);
            Debug.Log($"Found an extra counter to process? {toProcess != null}");
            Debug.Log($"Did we just trigger off normal counter? {willNormalTrigger}");
            if (toProcess && !willNormalTrigger)
            {
                // Reset the first extra counter and let any others stay at 0 for more triggers
                Debug.Log($"Resetting extra counter");
                toProcess.ResetCounter();
            }

            // Now that a counter has been reset (either the main or an extra), fire the hook for on reset
            if (SpirefrostEvents.HasCounterReset())
            {
                yield return SpirefrostEvents.CounterResetRoutine(entity);
                yield return Sequences.Wait(0.167f);
            }
        }

        internal static bool CheckCounters(Entity entity)
        {
            StatusEffectExtraCounter toProcess = ExtraCounterPatches.NextCustomCounterAtZero(entity);
            Debug.Log($"Actual counter is ready to trigger? {entity.counter.current <= 0}");
            Debug.Log($"An extra counter is ready to trigger? {toProcess != null}");
            if (entity.IsSnowed)
            {
                Debug.Log($"Entity snowed, cancel");
                return false;
            }
            if (entity.counter.current <= 0 || toProcess != null)
            {
                entity.PromptUpdate();
                return true;
            }
            return false;
        }

        internal static int LowestCounterValue(int current, Entity entity)
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

        static int state;

        static MethodBase TargetMethod()
        {
            return AccessTools.Method(enumeratorType, "MoveNext");
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            FieldInfo cancelTurn = AccessTools.Field(typeof(Battle), nameof(Battle.cancelTurn));
            FieldInfo unit = AccessTools.Field(enumeratorType, "<unit>5__4");
            FieldInfo eState = AccessTools.Field(enumeratorType, "<>1__state");
            FieldInfo eCurrent = AccessTools.Field(enumeratorType, "<>2__current");
            FieldInfo counter = AccessTools.Field(typeof(Entity), nameof(Entity.counter));
            FieldInfo patchState = AccessTools.Field(typeof(CheckUnitsPatch), nameof(state));
            MethodInfo promptUpdate = AccessTools.Method(typeof(Entity), nameof(Entity.PromptUpdate));
            MethodInfo check = AccessTools.Method(typeof(CounterResetLogic), nameof(CounterResetLogic.CheckCounters));
            MethodInfo process = AccessTools.Method(typeof(CounterResetLogic), nameof(CounterResetLogic.ProcessEntity));
            MethodInfo lowest = AccessTools.Method(typeof(CounterResetLogic), nameof(CounterResetLogic.LowestCounterValue));
            MethodInfo getCurr = AccessTools.Method(typeof(Stat), "get_current");
            MethodInfo getMax = AccessTools.Method(typeof(Stat), "get_max");
            MethodInfo setCurr = AccessTools.Method(typeof(Stat), "set_current");

            Label skipResetLabel = generator.DefineLabel();
            Label jumpBackLabel = generator.DefineLabel();
            CodeInstruction jumpBack = new CodeInstruction(OpCodes.Nop);
            jumpBack.labels.Add(jumpBackLabel);
            Label stateJumpLabel = generator.DefineLabel();
            CodeInstruction stateJump = new CodeInstruction(OpCodes.Nop);
            stateJump.labels.Add(stateJumpLabel);
            CodeInstruction leave = null;

            bool stateCheckInserted = false;
            bool jumpInserted = false;
            bool processInserted = false;
            bool skipInserted = false;
            bool sawSwitch = false;

            for (int i = 0; i < codes.Count; i++)
            {
                if (!sawSwitch && codes[i].opcode == OpCodes.Switch)
                {
                    sawSwitch = true;
                }
                if (leave == null && codes[i].IsLeave() && sawSwitch)
                {
                    MainModFile.Print("CheckUnitsPatch - Copying leave");
                    leave = codes[i];
                }
                if (!jumpInserted && codes[i].opcode == OpCodes.Ldloc_2 && i + 1 < codes.Count && codes[i + 1].opcode == OpCodes.Ldfld)
                {
                    if (codes[i + 1].operand is FieldInfo info && info == cancelTurn)
                    {
                        MainModFile.Print("CheckUnitsPatch - Match found, injecting jump back");
                        jumpInserted = true;
                        yield return jumpBack;
                    }
                }
                if (!processInserted && i + 1 < codes.Count && codes[i + 1].opcode == OpCodes.Callvirt)
                {
                    if (codes[i + 1].operand is MethodInfo info && info == promptUpdate && jumpInserted && i - 1 >= 0)
                    {
                        MainModFile.Print("CheckUnitsPatch - Match found, injecting new instructions and skip jump");
                        processInserted = true;
                        codes[i - 1].labels.Add(skipResetLabel);
                        // Ldarg0 already on the stack
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, codes[i].operand);
                        yield return new CodeInstruction(OpCodes.Call, process);
                        yield return new CodeInstruction(OpCodes.Stfld, eCurrent);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        yield return new CodeInstruction(OpCodes.Stsfld, patchState);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        yield return new CodeInstruction(OpCodes.Stloc_0);
                        yield return leave;
                        yield return stateJump;
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                        yield return new CodeInstruction(OpCodes.Stsfld, patchState);
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, codes[i].operand);
                        yield return new CodeInstruction(OpCodes.Call, check);
                        yield return new CodeInstruction(OpCodes.Brtrue, jumpBackLabel);
                        // Put Ldarg0 back on the stack
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
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
                if (!stateCheckInserted && codes[i].opcode == OpCodes.Stloc_2)
                {
                    MainModFile.Print("CheckUnitsPatch - match found, inserting state check");
                    stateCheckInserted = true;
                    yield return new CodeInstruction(OpCodes.Ldsfld, patchState);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return new CodeInstruction(OpCodes.Beq, stateJumpLabel);
                }
            }
        }
    }

    [HarmonyPatch]
    internal static class ProcessUnitPatch
    {
        private static Type createdType;

        static int state;

        static MethodBase TargetMethod()
        {
            return SpirefrostUtils.FindEnumeratorMethod(AccessTools.DeclaredMethod(typeof(Battle), "ProcessUnit", new Type[] { typeof(Entity) }), ref createdType);
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            FieldInfo counter = AccessTools.Field(typeof(Entity), "counter");
            FieldInfo unit = AccessTools.Field(createdType, "unit");
            FieldInfo eState = AccessTools.Field(createdType, "<>1__state");
            FieldInfo eCurrent = AccessTools.Field(createdType, "<>2__current");
            FieldInfo patchState = AccessTools.Field(typeof(CheckUnitsPatch), nameof(state));
            MethodInfo countDown = AccessTools.Method(typeof(Battle), "CardCountDown");
            MethodInfo promptUpdate = AccessTools.Method(typeof(Entity), "PromptUpdate");
            MethodInfo check = AccessTools.Method(typeof(CounterResetLogic), nameof(CounterResetLogic.CheckCounters));
            MethodInfo process = AccessTools.Method(typeof(CounterResetLogic), nameof(CounterResetLogic.ProcessEntity));
            MethodInfo lowest = AccessTools.Method(typeof(CounterResetLogic), "LowestCounterValue");
            MethodInfo getCurr = AccessTools.Method(typeof(Stat), "get_current");
            MethodInfo getMax = AccessTools.Method(typeof(Stat), "get_max");
            MethodInfo setCurr = AccessTools.Method(typeof(Stat), "set_current");

            Label skipResetLabel = generator.DefineLabel();
            Label jumpBackLabel = generator.DefineLabel();
            CodeInstruction jumpBack = new CodeInstruction(OpCodes.Nop);
            jumpBack.labels.Add(jumpBackLabel);
            Label stateJumpLabel = generator.DefineLabel();
            CodeInstruction stateJump = new CodeInstruction(OpCodes.Nop);
            stateJump.labels.Add(stateJumpLabel);

            bool jumpInserted = false;
            bool countDownFound = false;
            bool skipInserted = false;
            bool stateCheckInserted = false;

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
                if (jumpInserted && i + 1 < codes.Count && codes[i + 1].opcode == OpCodes.Callvirt)
                {
                    if (codes[i + 1].operand is MethodInfo info && info == promptUpdate && i - 1 >= 0)
                    {
                        Debug.Log("ProcessUnitPatch - Match found, injecting new instructions and skip jump");
                        codes[i - 1].labels.Add(skipResetLabel);
                        // Ldarg0 already on the stack
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, codes[i].operand);
                        yield return new CodeInstruction(OpCodes.Call, process);
                        yield return new CodeInstruction(OpCodes.Stfld, eCurrent);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        yield return new CodeInstruction(OpCodes.Stsfld, patchState);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        yield return new CodeInstruction(OpCodes.Ret);
                        yield return stateJump;
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                        yield return new CodeInstruction(OpCodes.Stsfld, patchState);
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, codes[i].operand);
                        yield return new CodeInstruction(OpCodes.Call, check);
                        yield return new CodeInstruction(OpCodes.Brtrue, jumpBackLabel);
                        // Put Ldarg0 back on the stack
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, codes[i].operand);
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
                if (!stateCheckInserted && codes[i].opcode == OpCodes.Stloc_1)
                {
                    MainModFile.Print("ProcessUnitPatch - match found, inserting state check");
                    stateCheckInserted = true;
                    yield return new CodeInstruction(OpCodes.Ldsfld, patchState);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return new CodeInstruction(OpCodes.Beq, stateJumpLabel);
                }
            }
        }
    }
}
