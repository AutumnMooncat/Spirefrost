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
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(Type.GetType("MonoMod.Utils._DMDEmit,MonoMod.Utils"), "Generate");
            yield return AccessTools.Method(Type.GetType("Battle+<CheckUnitsTakeTurns>d__73,Assembly-CSharp"), "MoveNext");
            yield return AccessTools.Method(typeof(Module), "get_Assembly");
        }

        static MemberInfo lastSeen;

        static Assembly AssemblyRedirection(Module module)
        {
            try
            {
                // Do RuntimeModule cast and return that
                Type runtimeModuleType = Type.GetType("System.Reflection.RuntimeModule,mscorlib");
                if (module.GetType() == runtimeModuleType)
                {
                    return (Assembly)AccessTools.PropertyGetter(runtimeModuleType, "Assembly").Invoke(module, null);
                }
                
                // Attempt a normal return, which could throw if it uses the same logic as MonoMod
                Assembly ass = module.Assembly;
                return ass;
            }
            catch (Exception ex)
            {
                Debug.Log($"Module {module} failed to get Assembly, dumping info:");
                Debug.Log($"Module Name: {module.Name}");
                Debug.Log($"Module Type: {module.GetType()}");
                throw ex;
            }
        }

        // Used by getAssembly patch instead of creating a new local variable
        static Assembly foundAssembly;

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            if (original.Name == "Generate")
            {
                MethodInfo getAssembly = AccessTools.Method(typeof(Module), "get_Assembly");
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call)
                    {
                        if (codes[i].operand is MethodInfo info && info == getAssembly)
                        {
                            // Add call to my method to properly put an Assembly on the stack
                            CodeInstruction call = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CheckUnitsPatch), "AssemblyRedirection"));
                            call.labels.AddRange(codes[i].labels);
                            codes[i].labels.Clear();
                            yield return call;

                            // Skip original code as it throws an exception
                            continue;
                        }
                    }
                    yield return codes[i];
                }
            }
            else if (original.Name == "get_Assembly")
            {
                Label jump = generator.DefineLabel();
                FieldInfo foundAssemblyField = AccessTools.Field(typeof(CheckUnitsPatch), "foundAssembly");

                // Add call to my method and return the assembly it finds
                MainModFile.Print($"CheckUnitsPatch - Get Assembly injection");
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CheckUnitsPatch), "AssemblyRedirection"));
                yield return new CodeInstruction(OpCodes.Stsfld, foundAssemblyField);
                yield return new CodeInstruction(OpCodes.Ldsfld, foundAssemblyField);
                yield return new CodeInstruction(OpCodes.Brfalse, jump);
                yield return new CodeInstruction(OpCodes.Ldsfld, foundAssemblyField);
                yield return new CodeInstruction(OpCodes.Ret);

                // Jump to original exception throwing code if my method returns null (it doesnt, it also throws, but as a safety measure)
                codes[0].labels.Add(jump);
                for (int i = 0; i < codes.Count; i++)
                {
                    yield return codes[i];
                }
            }
            else
            {
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
                            yield return new CodeInstruction(OpCodes.Ldfld, codes[i-1].operand);
                        }
                    }
                    yield return codes[i];
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
