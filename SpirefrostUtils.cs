using HarmonyLib;
using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using UnityEngine;

namespace Spirefrost
{
    internal class SpirefrostUtils
    {
        internal class WeightedString : IComparable<WeightedString>
        {
            public WeightedString(string str, int weight)
            {
                this.str = str;
                this.weight = weight;
            }

            public string str;
            public int weight;

            public int CompareTo(WeightedString other)
            {
                int comp = weight.CompareTo(other.weight);
                if (comp != 0)
                {
                    return comp;
                }
                return str.CompareTo(other.str);
            }
        }

        internal class AutoAdd
        {

            [AttributeUsage(AttributeTargets.Class)]
            internal class Ignore : Attribute { }

            [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
            internal class ToPoolList : Attribute 
            {
                public ToPoolList(MainModFile.PoolListType type) : this(type, 1, 0) { }

                public ToPoolList(MainModFile.PoolListType type, int copies, int weight)
                {
                    this.type = type;
                    this.copies = copies;
                    this.weight = weight;
                }

                public MainModFile.PoolListType type;
                public int copies;
                public int weight;

                public void Process(string id)
                {
                    List<WeightedString> pool = MainModFile.instance.poolData.GetValueOrDefault(type, new List<WeightedString>());
                    for (int i = 0; i < copies; i++)
                    {
                        pool.Add(new WeightedString(id, weight));
                    }
                    pool.Sort();
                    MainModFile.instance.poolData[type] = pool;
                }
            }

            private IEnumerable<Type> GetAll(Type type)
            {
                return AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(t => t.IsSubclassOf(type) && !Attribute.IsDefined(t, typeof(Ignore)));
            }

            internal List<object> Process(Type type, string methodName, string fieldName)
            {
                List<object> result = new List<object>();
                foreach (Type t in GetAll(type))
                {
                    object obj = (t.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic)?.Invoke(null, null)) 
                        ?? throw new Exception($"AutoAdd Error: Type {t} does not define static method {methodName}");
                    result.Add(obj);

                    string id = (string)(t.GetProperty(fieldName, BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null)) 
                        ?? throw new Exception($"AutoAdd Error: Type {t} does not define static property {fieldName}");

                    Attribute[] attributes = Attribute.GetCustomAttributes(t);
                    foreach (Attribute attr in attributes)
                    {
                        if (attr is ToPoolList toList)
                        {
                            toList.Process(id);
                        }
                    }
                }
                return result;
            }
        }

        // HarmonyX logic
        internal static MethodBase FindEnumeratorMethod(MethodBase enumerator, ref Type foundType)
        {
            if (enumerator is null)
            {
                Debug.Log($"FindEnumeratorMethod - enumerator is null");
                return null;
            }

            ILContext ctx = new ILContext(new DynamicMethodDefinition(enumerator).Definition);
            ILCursor il = new ILCursor(ctx);

            Type enumeratorType;
            if (ctx.Method.ReturnType.Name.StartsWith("UniTask"))
            {
                TypeReference firstVar = ctx.Body.Variables.FirstOrDefault()?.VariableType;
                if (firstVar is object && !firstVar.Name.Contains(enumerator.Name))
                {
                    Debug.Log($"FindEnumeratorMethod - Unexpected type name {firstVar.Name}, should contain {enumerator.Name}");
                    return null;
                }

                enumeratorType = firstVar.ResolveReflection();
            }
            else
            {
                MethodReference enumeratorCtor = null;
                il.GotoNext(i => i.MatchNewobj(out enumeratorCtor));
                if (enumeratorCtor is null)
                {
                    Debug.Log($"FindEnumeratorMethod - {enumerator.FullDescription()} does not create enumerators");
                    return null;
                }

                if (enumeratorCtor.Name != ".ctor")
                {
                    Debug.Log($"FindEnumeratorMethod - {enumerator.FullDescription()} does not create an enumerator (got {enumeratorCtor.GetID(simple: true)})");
                    return null;
                }

                enumeratorType = enumeratorCtor.DeclaringType.ResolveReflection();
                Debug.Log($"FindEnumeratorMethod - Found Type: {enumeratorType}");
                foundType = enumeratorType;
            }

            MethodInfo moveNext = enumeratorType.GetMethod(nameof(IEnumerator.MoveNext), AccessTools.all);
            if (moveNext is null)
            {
                Debug.Log($"FindEnumeratorMethod - {enumerator.FullDescription()} creates an object {enumeratorType.FullDescription()} but it doesn't have MoveNext");
                return null;
            }

            Debug.Log($"FindEnumeratorMethod - Found Method: {moveNext}");
            return moveNext;
        }
    }
}
