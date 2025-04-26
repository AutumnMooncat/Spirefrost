using HarmonyLib;
using Mono.Cecil;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Spirefrost.Patches
{
    internal class HarmonyInsertPatch<T>
    {
        internal HarmonyInsertPatch(IEnumerable<CodeInstruction> instructions, MethodBase original, ILGenerator generator)
        {
            _original = original;
            _generator = generator;
            _newCodes = new List<CodeInstruction>(instructions);
            _originalCodes = new List<CodeInstruction>(instructions);
        }

        private readonly MethodBase _original;
        private readonly ILGenerator _generator;
        private readonly List<CodeInstruction> _newCodes;
        private readonly List<CodeInstruction> _originalCodes;
        private readonly List<string> _paramArgs = new List<string>();
        private readonly List<string> _localArgs = new List<string>();
        private readonly List<int> _indices = new List<int>();
        private bool _replaces;

        internal HarmonyInsertPatch<T> WithIndices(int[] indices)
        {
            _indices.Clear();
            _indices.AddRange(indices);
            return this;
        }

        internal HarmonyInsertPatch<T> FindFirstMatch(Matcher.OpcodeMatcher m)
        {
            _indices.Clear();
            _indices.AddRange(m.FindInOrder(_newCodes));
            return this;
        }

        internal HarmonyInsertPatch<T> FindAllMatches(Matcher.OpcodeMatcher m)
        {
            _indices.Clear();
            _indices.AddRange(m.FindAllInOrder(_newCodes));
            return this;
        }

        internal HarmonyInsertPatch<T> FindSpecificMatch(Matcher.OpcodeMatcher m, int n)
        {
            _indices.Clear();
            _indices.Add(m.FindAllInOrder(_newCodes)[n]);
            return this;
        }

        internal HarmonyInsertPatch<T> WithParams(params string[] args)
        {
            _paramArgs.Clear();
            _paramArgs.AddRange(args);
            return this;
        }

        internal HarmonyInsertPatch<T> WithLocals(params string[] args)
        {
            _localArgs.Clear();
            _localArgs.AddRange(args);
            return this;
        }

        internal HarmonyInsertPatch<T> ReplacesMatch(bool replaces = true)
        {
            _replaces = replaces;
            return this;
        }

        internal HarmonyInsertPatch<T> ApplyPatch(MethodInfo toProcess)
        {
            MainModFile.Print($"Patching {toProcess} into {_original}");
            MainModFile.Print($"Found {_indices.Count} matches: {_indices.Join()}");
            MainModFile.Print($"");
            List<CodeInstruction> toInject = CodesFromMethod(toProcess);
            if (toProcess.ReturnType == typeof(HarmonyReturn))
            {
                MainModFile.Print($"Detected HarmonyReturn, loading return codes");
                FieldInfo retField = AccessTools.Field(typeof(HarmonyReturn), "_returnHolder");
                Label dontReturn = _generator.DefineLabel();
                toInject.Add(new CodeInstruction(OpCodes.Stsfld, retField));
                toInject.Add(new CodeInstruction(OpCodes.Ldsfld, retField));
                toInject.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyReturn), "Present")));
                toInject.Add(new CodeInstruction(OpCodes.Brfalse, dontReturn));
                toInject.Add(new CodeInstruction(OpCodes.Ldsfld, retField));
                toInject.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyReturn), "IsVoid")));
                toInject.Add(new CodeInstruction(OpCodes.Brtrue, dontReturn));
                toInject.Add(new CodeInstruction(OpCodes.Ldsfld, retField));
                toInject.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyReturn), "Get")));
                toInject.Add(new CodeInstruction(OpCodes.Ret));
                CodeInstruction nop = new CodeInstruction(OpCodes.Nop);
                nop.labels.Add(dontReturn);
                toInject.Add(nop);
            }
            foreach (int index in _indices)
            {
                MainModFile.Print($"Processing index {index}");
                List<CodeInstruction> toRestore = StackRestore(index);
                MainModFile.Print($"Noping {toRestore.Count} stored instructions");
                for (int i = 0; i < toRestore.Count; i++)
                {
                    _newCodes[index-i-1] = new CodeInstruction(OpCodes.Nop);
                }
                if (!_replaces)
                {
                    MainModFile.Print($"Storing original instruction");
                    toRestore.Add(_newCodes[index]);
                }
                MainModFile.Print($"Noping original instruction, advancing index");
                _newCodes[index] = new CodeInstruction(OpCodes.Nop);
                MainModFile.Print($"Remapping labels");
                int offset = _replaces ? 0 : 1;
                for (int i = 0; i < toRestore.Count; i++)
                {
                    _newCodes[index - offset].labels.AddRange(toRestore[toRestore.Count-1-i].labels);
                    toRestore[toRestore.Count - 1 - i].labels.Clear();
                }
                int insertAt = index + 1;
                MainModFile.Print($"Injecting {toInject.Count} new instructions and {toRestore.Count} restored instructions at index {insertAt}");
                _newCodes.InsertRange(insertAt, toRestore);
                _newCodes.InsertRange(insertAt, toInject);
            }
            return this;
        }

        internal IEnumerable<CodeInstruction> Compile()
        {
            MainModFile.Print($"Dumping original method instructions");
            MainModFile.Print($"");
            MainModFile.Print($"Instructions:");
            foreach (var instruction in _originalCodes)
            {
                MainModFile.Print($"Opcode: {instruction.opcode}, Operand: {instruction.operand}");
            }
            MainModFile.Print($"");

            MainModFile.Print($"Returning compiled instructions:");
            foreach (var instruction in _newCodes)
            {
                MainModFile.Print($"Opcode: {instruction.opcode}, Operand: {instruction.operand}");
            }
            MainModFile.Print($"");
            return _newCodes;
        }

        private List<CodeInstruction> StackRestore(int index)
        {
            List<CodeInstruction> restore = new List<CodeInstruction>();
            List<Type> toFind = GetRequiredTypes(_newCodes[index]);
            MainModFile.Print($"Got initial requirements: {toFind.Join()}");
            while (toFind.Count > 0 && index > 0) 
            {
                index--;
                CodeInstruction current = _newCodes[index];
                restore.Insert(0, current);
                Type result = GetResultingType(current);
                MainModFile.Print($"Found {result} to check, looking for {toFind[toFind.Count - 1]}");
                if (toFind[toFind.Count - 1].IsAssignableFrom(result))
                {
                    MainModFile.Print($"Match found, removing {toFind[toFind.Count - 1]}");
                    toFind.RemoveAt(toFind.Count-1);
                }
                MainModFile.Print($"Looking for more requirements");
                toFind.AddRange(GetRequiredTypes(current));
            }

            if (toFind.Count > 0)
            {
                throw new Exception($"Insert patch failed to find types: {toFind.Join()}");
            }

            return restore;
        }

        private List<Type> GetRequiredTypes(CodeInstruction instruction)
        {
            List<Type> required = new List<Type>();
            if (instruction.operand is FieldInfo fi && !fi.IsStatic)
            {
                MainModFile.Print($"Added {fi.DeclaringType} to required list");
                required.Add(fi.DeclaringType);
            }
            else if (instruction.operand is MethodInfo mi)
            {
                if (!mi.IsStatic)
                {
                    MainModFile.Print($"Added {mi.DeclaringType} to required list");
                    required.Add(mi.DeclaringType);
                }
                foreach (var item in mi.GetParameters())
                {
                    MainModFile.Print($"Added {item.ParameterType} to required list");
                    required.Add(item.ParameterType);
                    if (item.ParameterType.IsByRef)
                    {
                        MainModFile.Print($"Param {item} is by ref, actual type is {item.ParameterType}");
                    }
                }
            }
            return required;
        }

        private Type GetResultingType(CodeInstruction instruction)
        {
            if (instruction.operand is FieldInfo fi)
            {
                return fi.FieldType;
            }
            if (instruction.operand is MethodInfo mi)
            {
                if (mi.ReturnType.Name == typeof(void).Name)
                {
                    return null;
                }
                return mi.ReturnType;
            }

            if (instruction.opcode == OpCodes.Ldarg)
            {
                if ((int)instruction.operand-1 == -1)
                {
                    return _original.DeclaringType;
                }
                return _original.GetParameters()[(byte)instruction.operand-1].ParameterType;
            }
            if (instruction.opcode == OpCodes.Ldarga)
            {
                if ((byte)instruction.operand - 1 == -1)
                {
                    return _original.DeclaringType;
                }
                MainModFile.Print($"Loaded {_original.GetParameters()[(byte)instruction.operand-1]} as ref, actual type is {_original.GetParameters()[(byte)instruction.operand-1].ParameterType}");
                return _original.GetParameters()[(byte)instruction.operand-1].ParameterType;
            }
            if (instruction.opcode == OpCodes.Ldarga_S)
            {
                if ((byte)instruction.operand - 1 == -1)
                {
                    return _original.DeclaringType;
                }
                MainModFile.Print($"Loaded {_original.GetParameters()[(byte)instruction.operand-1]} as ref, actual type is {_original.GetParameters()[(byte)instruction.operand-1].ParameterType}");
                return _original.GetParameters()[(byte)instruction.operand-1].ParameterType;
            }
            if (instruction.opcode == OpCodes.Ldarg_0)
            {
                return _original.DeclaringType;
            }
            if (instruction.opcode == OpCodes.Ldarg_1)
            {
                return _original.GetParameters()[0].ParameterType;
            }
            if (instruction.opcode == OpCodes.Ldarg_2)
            {
                return _original.GetParameters()[1].ParameterType;
            }
            if (instruction.opcode == OpCodes.Ldarg_3)
            {
                return _original.GetParameters()[2].ParameterType;
            }
            if (instruction.opcode == OpCodes.Ldarg_S)
            {
                if ((int)instruction.operand - 1 == -1)
                {
                    return _original.DeclaringType;
                }
                return _original.GetParameters()[(byte)instruction.operand-1].ParameterType;
            }
            if (instruction.opcode == OpCodes.Ldloc)
            {
                return _original.GetMethodBody().LocalVariables[(byte)instruction.operand].LocalType;
            }
            if (instruction.opcode == OpCodes.Ldloca)
            {
                MainModFile.Print($"Loaded {_original.GetMethodBody().LocalVariables[(byte)instruction.operand]} as ref, actual type is {_original.GetMethodBody().LocalVariables[(byte)instruction.operand]}");
                return _original.GetMethodBody().LocalVariables[(byte)instruction.operand].LocalType;
            }
            if (instruction.opcode == OpCodes.Ldloca_S)
            {
                MainModFile.Print($"Loaded {_original.GetMethodBody().LocalVariables[(byte)instruction.operand]} as ref, actual type is {_original.GetMethodBody().LocalVariables[(byte)instruction.operand]}");
                return _original.GetMethodBody().LocalVariables[(byte)instruction.operand].LocalType;
            }
            if (instruction.opcode == OpCodes.Ldloc_0)
            {
                return _original.GetMethodBody().LocalVariables[0].LocalType;
            }
            if (instruction.opcode == OpCodes.Ldloc_1)
            {
                return _original.GetMethodBody().LocalVariables[1].LocalType;
            }
            if (instruction.opcode == OpCodes.Ldloc_2)
            {
                return _original.GetMethodBody().LocalVariables[2].LocalType;
            }
            if (instruction.opcode == OpCodes.Ldloc_3)
            {
                return _original.GetMethodBody().LocalVariables[3].LocalType;
            }
            if (instruction.opcode == OpCodes.Ldloc_S)
            {
                return _original.GetMethodBody().LocalVariables[(byte)instruction.operand].LocalType;
            }
            if (LoadsInt(instruction.opcode))
            {
                return typeof(int);
            }
            if (LoadsFloat(instruction.opcode))
            {
                return typeof(float);
            }
            return null;
        }

        private bool LoadsInt(OpCode opcode)
        {
            if (opcode == OpCodes.Ldc_I4
                || opcode == OpCodes.Ldc_I4_0
                || opcode == OpCodes.Ldc_I4_1
                || opcode == OpCodes.Ldc_I4_2
                || opcode == OpCodes.Ldc_I4_3
                || opcode == OpCodes.Ldc_I4_4
                || opcode == OpCodes.Ldc_I4_5
                || opcode == OpCodes.Ldc_I4_6
                || opcode == OpCodes.Ldc_I4_7
                || opcode == OpCodes.Ldc_I4_8
                || opcode == OpCodes.Ldc_I4_M1
                || opcode == OpCodes.Ldc_I4_S
                || opcode == OpCodes.Ldc_I8)
            {
                return true;
            }
            return false;
        }

        private bool LoadsFloat(OpCode opcode)
        {
            if (opcode == OpCodes.Ldc_R4
                || opcode == OpCodes.Ldc_R8)
            {
                return true;
            }
            return false;
        }

        private List<CodeInstruction> CreatePops(CodeInstruction targetInstruction)
        {
            List<CodeInstruction> pops = new List<CodeInstruction>();
            if (targetInstruction.operand is FieldInfo fi && !fi.IsStatic)
            {
                // Pop if not static
                pops.Add(new CodeInstruction(OpCodes.Pop));
            }
            else if (targetInstruction.operand is MethodInfo mi)
            {
                // Pop the caller, if not static
                if (!mi.IsStatic)
                {
                    pops.Add(new CodeInstruction(OpCodes.Pop));
                }
                // Pop each param
                for (int i = 0; i < mi.GetParameters().Length; i++)
                {
                    pops.Add(new CodeInstruction(OpCodes.Pop));
                }
            }
            return pops;
        }

        private List<CodeInstruction> CodesFromMethod(MethodInfo toProcess)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>();
            DynamicMethodDefinition dynMethod = new DynamicMethodDefinition(toProcess);

            Type instanceType = toProcess.GetParameters().First().ParameterType;
            if (_original.DeclaringType == instanceType)
            {
                // Grab __instance directly
                MainModFile.Print($"Loaded instance directly");
                codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
            }
            else
            {
                // We are in a nested class, grab instance from local
                LocalVariableInfo info = _original.GetMethodBody().LocalVariables.First(p => p.LocalType == instanceType) ?? throw new Exception($"Insert patch failed to find instance");
                MainModFile.Print($"Loaded instance from local");
                codes.Add(new CodeInstruction(OpCodes.Ldloc, info.LocalIndex));
            }

            List<string> missingParams = new List<string>();
            foreach (var name in _paramArgs)
            {
                var neededParam = dynMethod.Definition.Parameters.First(p => p.Name == name);
                if (neededParam == null)
                {
                    missingParams.Add(name);
                    continue;
                }
                var actualParam = _original.GetParameters().First(p => p.Name == name);
                if (actualParam == null)
                {
                    missingParams.Add(name);
                    continue;
                }
                // Add 1 to param position to account for `this` as arg0
                MainModFile.Print($"Loading param {name}, is it byref? {neededParam.ParameterType.IsByReference}, was it already by ref? {actualParam.ParameterType.IsByRef}");
                if (neededParam.ParameterType.IsByReference && !actualParam.ParameterType.IsByRef)
                {
                    MainModFile.Print($"Loaded param {name} by ref");
                    codes.Add(new CodeInstruction(OpCodes.Ldarga, actualParam.Position+1));
                }
                else
                {
                    MainModFile.Print($"Loaded param {name}");
                    codes.Add(new CodeInstruction(OpCodes.Ldarg, actualParam.Position+1));
                }
            }
            List<ParameterDefinition> locals = new List<ParameterDefinition>();
            foreach (var name in _localArgs)
            {
                var neededParam = dynMethod.Definition.Parameters.First(p => p.Name == name);
                if (neededParam == null)
                {
                    missingParams.Add(name);
                    continue;
                }
                locals.Add(neededParam);
            }
            if (locals.Count > 0)
            {
                foreach (var local in _original.GetMethodBody().LocalVariables)
                {
                    MainModFile.Print($"Looking for local {locals[0]}, is it byref? {locals[0].ParameterType.IsByReference}");
                    if (local.LocalType.Name == locals.First().ParameterType.Name)
                    {
                        MainModFile.Print($"Loaded local {locals[0]}");
                        codes.Add(new CodeInstruction(OpCodes.Ldloc, local.LocalIndex));
                        locals.RemoveAt(0);
                    }
                    else if (local.LocalType.Name + "&" == locals.First().ParameterType.Name)
                    {
                        MainModFile.Print($"Loaded local {locals[0]} by ref");
                        codes.Add(new CodeInstruction(OpCodes.Ldloca, local.LocalIndex));
                        locals.RemoveAt(0);
                    }
                }
            }
            missingParams.AddRange(locals.Select(p => p.Name));
            if (missingParams.Count > 0)
            {
                throw new Exception($"Insert patch failed, could not assign parameters: {missingParams.Aggregate("", (prev, curr) => prev + curr)}");
            }
            MainModFile.Print($"Loaded method call");
            codes.Add(new CodeInstruction(OpCodes.Call, toProcess));
            return codes;
        }
    }

    internal class HarmonyReturn
    {
        private static HarmonyReturn _returnHolder;

        private HarmonyReturn() 
        {
            _value = null;
            _hasValue = false;
            _isVoid = false;
        }

        private HarmonyReturn(object value)
        {
            _value = value;
            _hasValue = true;
            _isVoid = value == null;
        }

        private readonly object _value;
        private readonly bool _hasValue;
        private readonly bool _isVoid;

        internal static HarmonyReturn Continue()
        {
            return new HarmonyReturn();
        }

        internal static HarmonyReturn Return(object value)
        {
            return new HarmonyReturn(value);
        }

        internal static HarmonyReturn Return()
        {
            return new HarmonyReturn(null);
        }

        public bool Present()
        {
            return _hasValue;
        }

        public object Get()
        {
            return _value;
        }

        public bool IsVoid()
        {
            return _isVoid;
        }
    }

    internal class Matcher
    {
        internal class OpcodeMatcher
        {
            internal OpcodeMatcher(OpCode code) : this(code, null) { }
            internal OpcodeMatcher(OpCode code, object operand) : this(code, new object[] { operand }) { }

            internal OpcodeMatcher(OpCode code, object[] operands) : this(new List<KeyValuePair<OpCode, object[]>>() { new KeyValuePair<OpCode, object[]>(code, operands) }) { }

            internal OpcodeMatcher(List<KeyValuePair<OpCode, object[]>> codeOperandsPairs)
            {
                _matchers = codeOperandsPairs;
            }

            private readonly List<KeyValuePair<OpCode, object[]>> _matchers;

            internal int[] FindInOrder(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count(); i++)
                {
                    foreach (var pair in _matchers)
                    {
                        foreach (var obj in pair.Value)
                        {
                            if (codes[i].opcode == pair.Key && codes[i].operand == obj)
                            {
                                MainModFile.Print($"Matcher found index {i}");
                                return new int[] { i };
                            }
                        }
                    }
                }
                MainModFile.Print($"Matcher failed to match");
                return new int[0];
            }

            internal int[] FindAllInOrder(IEnumerable<CodeInstruction> instructions)
            {
                List<int> matches = new List<int>();
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count(); i++)
                {
                    foreach (var pair in _matchers)
                    {
                        foreach (var obj in pair.Value)
                        {
                            if (codes[i].opcode == pair.Key && codes[i].operand == obj)
                            {
                                if (!matches.Contains(i))
                                {
                                    matches.Add(i);
                                }
                            }
                        }
                    }
                }
                return matches.ToArray();
            }
        }

        internal class FieldAccessMatcher : OpcodeMatcher
        {
            internal FieldAccessMatcher(FieldInfo info) :  
                base(new List<KeyValuePair<OpCode, object[]>>() 
                { 
                    new KeyValuePair<OpCode, object[]>(OpCodes.Ldfld, new object[] { info }),
                    new KeyValuePair<OpCode, object[]>(OpCodes.Ldflda, new object[] { info })
                }) 
            { }
        }

        internal class MethodCallMatcher : OpcodeMatcher
        {
            internal MethodCallMatcher(MethodInfo info) :
                base(new List<KeyValuePair<OpCode, object[]>>()
                {
                    new KeyValuePair<OpCode, object[]>(OpCodes.Call, new object[] { info }),
                    new KeyValuePair<OpCode, object[]>(OpCodes.Callvirt, new object[] { info })
                })
            { }
        }
    }
}
