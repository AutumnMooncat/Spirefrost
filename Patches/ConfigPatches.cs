using HarmonyLib;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using static Deadpan.Enums.Engine.Components.Modding.WildfrostMod;
using static UnityEngine.Rendering.DebugUI;

namespace Spirefrost.Patches
{
    [HarmonyPatch]
    internal class ConfigPatches
    {
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
        internal class ConfigManagerSetting : Attribute
        {
            internal enum SettingType
            {
                HideIf,
                ShowIf
            }

            internal ConfigManagerSetting(SettingType type, string listeningTo, params object[] acceptedValues)
            {
                this.type = type;
                this.listeningTo = listeningTo;
                this.acceptedValues = acceptedValues;
            }

            internal SettingType type;
            internal string listeningTo;
            internal object[] acceptedValues;

            internal bool IsSatisfied(object value)
            {
                return acceptedValues.Contains(value);
            }
        }

        private static Type configItemType;
        private static MethodBase configItemSVL;

        static bool Prepare(MethodBase original)
        {
            if (original == null)
            {
                configItemType = Type.GetType("WildfrostHopeMod.Configs.ConfigItem,Config Manager");
                configItemSVL = AccessTools.Method(configItemType, "SetVisibilityListeners");
                if (configItemSVL != null)
                {
                    MainModFile.Print($"Config Manager patched");
                    return true;
                }
                MainModFile.Print($"Config Manager not patched");
                return false;
            }
            return true;
        }

        static MethodBase TargetMethod()
        {
            return configItemSVL;
        }

        static void Postfix(object __instance)
        {
            (ConfigItemAttribute attr, FieldInfo fld) = ((ConfigItemAttribute, FieldInfo))AccessTools.Field(configItemType, "con").GetValue(__instance);
            ConfigManagerSetting[] settings = Attribute.GetCustomAttributes(fld, typeof(ConfigManagerSetting)) as ConfigManagerSetting[];
            string fieldName = (string)AccessTools.Field(configItemType, "fieldName").GetValue(__instance);
            Type parentType = AccessTools.Field(configItemType, "parent").FieldType;
            object parentObj = AccessTools.Field(configItemType, "parent").GetValue(__instance);
            IDictionary items = AccessTools.Field(parentType, "items").GetValue(parentObj) as IDictionary;
            IDictionary hiders = AccessTools.Field(configItemType, "hiders").GetValue(__instance) as IDictionary;
            IDictionary showers = AccessTools.Field(configItemType, "showers").GetValue(__instance) as IDictionary;
            Type configSetType = typeof(HashSet<>).MakeGenericType(configItemType);
            EventInfo configEvent = parentType.GetEvent("OnConfigChanged", AccessTools.all);
            MethodInfo addMethod = configEvent.AddMethod;
            Type delClasstype = typeof(DelClass<,>).MakeGenericType(configItemType, typeof(object));
            foreach (var item in settings)
            {
                if (item.listeningTo != fieldName && items.Contains(item.listeningTo))
                {
                    switch(item.type)
                    {
                        case ConfigManagerSetting.SettingType.HideIf:
                            if (!hiders.Contains(0))
                            {
                                hiders[0] = Activator.CreateInstance(configSetType);
                            }

                            object hideDel = Activator.CreateInstance(delClasstype, __instance, hiders, item);
                            addMethod.Invoke(parentObj, new object[] { Delegate.CreateDelegate(configEvent.EventHandlerType, hideDel, "Run") });
                            
                            break;
                        case ConfigManagerSetting.SettingType.ShowIf:
                            if (!showers.Contains(0))
                            {
                                showers[0] = Activator.CreateInstance(configSetType);
                            }

                            object showDel = Activator.CreateInstance(delClasstype, __instance, showers, item);
                            addMethod.Invoke(parentObj, new object[] { Delegate.CreateDelegate(configEvent.EventHandlerType, showDel, "Run") });

                            break;
                    }
                }
            }
        }

        internal class DelClass<T,O>
        {
            public DelClass(object __instance, IDictionary dict, ConfigManagerSetting setting)
            {
                this.__instance = __instance;
                this.dict = dict;
                this.setting = setting;
            }

            readonly object __instance;
            readonly Type dictValType = typeof(HashSet<>).MakeGenericType(configItemType);
            readonly IDictionary dict;
            readonly ConfigManagerSetting setting;

            public void Run(T item, O val)
            {
                object fieldName = AccessTools.Field(configItemType, "fieldName").GetValue(item);
                Debug.Log($"We actually did something? Changed Field: {fieldName}, New Value: {val}, Satisfied: {setting.IsSatisfied(val)}");
                if (!setting.IsSatisfied(val))
                {
                    AccessTools.Method(dictValType, "Remove").Invoke(dict[0], new object[] { item });
                }
                else
                {
                    AccessTools.Method(dictValType, "Add").Invoke(dict[0], new object[] { item });
                }
                AccessTools.Method(configItemType, "UpdateVisibility").Invoke(__instance, new object[] { false, true });
            }
        }

        static Exception Cleanup(MethodBase original, Exception exception)
        {
            if (exception != null)
            {
                MainModFile.Print($"Patching Config Manager Failed:");
                Debug.Log(exception);
            }
            return null;
        }
    }
}
