using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine;

namespace Spirefrost.Patches
{
    internal class SafetyPatches
    {
        // Likely no longer needed now that the safety patch always applies first and removes last
        internal static MethodInfo DMDEmit_Generate = AccessTools.Method(Type.GetType("MonoMod.Utils._DMDEmit,MonoMod.Utils"), "Generate");
        internal static MethodInfo DMDEmit_Generate_Patch = AccessTools.Method(typeof(SafetyPatches), nameof(DMDEmit_Generate_Transpiler));

        // Needed for some transpilers patches to not crash
        internal static MethodInfo Module_GetAssembly = AccessTools.Method(typeof(Module), "get_Assembly");
        internal static MethodInfo Module_GetAssembly_Patch = AccessTools.Method(typeof(SafetyPatches), nameof(Module_GetAssembly_Transpiler));

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

        static IEnumerable<CodeInstruction> DMDEmit_Generate_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            MethodInfo getAssembly = AccessTools.Method(typeof(Module), "get_Assembly");
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call)
                {
                    if (codes[i].operand is MethodInfo info && info == getAssembly)
                    {
                        // Add call to my method to properly put an Assembly on the stack
                        CodeInstruction call = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SafetyPatches), nameof(AssemblyRedirection)));
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

        static IEnumerable<CodeInstruction> Module_GetAssembly_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            Label jump = generator.DefineLabel();
            LocalBuilder local = generator.DeclareLocal(typeof(Assembly));
            local.SetLocalSymInfo("foundAssembly");

            // Add call to my method and return the assembly it finds
            MainModFile.Print($"SafetyPatches - Get Assembly injection");
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SafetyPatches), nameof(AssemblyRedirection)));
            yield return new CodeInstruction(OpCodes.Stloc, local.LocalIndex);
            yield return new CodeInstruction(OpCodes.Ldloc, local.LocalIndex);
            yield return new CodeInstruction(OpCodes.Brfalse, jump);
            yield return new CodeInstruction(OpCodes.Ldloc, local.LocalIndex);
            yield return new CodeInstruction(OpCodes.Ret);

            // Jump to original exception throwing code if my method returns null (it doesnt, it also throws, but as a safety measure)
            codes[0].labels.Add(jump);
            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
            }
        }
    }
}
