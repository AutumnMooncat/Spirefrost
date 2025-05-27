using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using static Deadpan.Enums.Engine.Components.Modding.WildfrostMod;

namespace Spirefrost.Patches
{
    internal class ConfigPatches
    {
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
        internal class ConfigManagerVisibilitySetting : Attribute
        {
            internal enum SettingType
            {
                HideIf,
                ShowIf
            }

            internal ConfigManagerVisibilitySetting(SettingType type, string listeningTo, params object[] acceptedValues)
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

        [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
        internal class ConfigManagerCallbackSetting : Attribute
        {
            internal ConfigManagerCallbackSetting(string callbackName)
            {
                this.callbackName = callbackName;
            }

            internal string callbackName;
        }

        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        internal class ConfigManagerOptionSetting : Attribute
        {
            public ConfigManagerOptionSetting(string[] labels, object[] vals)
            {
                this.labels = labels;
                this.vals = vals;
            }

            internal string[] labels;
            internal object[] vals;
        }

        [HarmonyPatch]
        internal static class SetVisibilityListenersPatch
        {
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
                        MainModFile.Print($"Config Manager ConfigItem.SetVisibilityListeners patched");
                        return true;
                    }
                    MainModFile.Print($"Config Manager ConfigItem.SetVisibilityListeners not patched");
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
                ConfigManagerVisibilitySetting[] settings = Attribute.GetCustomAttributes(fld, typeof(ConfigManagerVisibilitySetting)) as ConfigManagerVisibilitySetting[];
                string fieldName = (string)AccessTools.Field(configItemType, "fieldName").GetValue(__instance);
                Type parentType = AccessTools.Field(configItemType, "parent").FieldType;
                object parentObj = AccessTools.Field(configItemType, "parent").GetValue(__instance);
                IDictionary items = AccessTools.Field(parentType, "items").GetValue(parentObj) as IDictionary;
                IDictionary hiders = AccessTools.Field(configItemType, "hiders").GetValue(__instance) as IDictionary;
                IDictionary showers = AccessTools.Field(configItemType, "showers").GetValue(__instance) as IDictionary;
                Type configSetType = typeof(HashSet<>).MakeGenericType(configItemType);
                EventInfo configEvent = parentType.GetEvent("OnConfigChanged", AccessTools.all);
                MethodInfo addMethod = configEvent.AddMethod;
                Type visiblityDelClassType = typeof(VisibilityDelClass<,>).MakeGenericType(configItemType, typeof(object));
                foreach (var item in settings)
                {
                    if (item.listeningTo != fieldName && items.Contains(item.listeningTo))
                    {
                        switch (item.type)
                        {
                            case ConfigManagerVisibilitySetting.SettingType.HideIf:
                                if (!hiders.Contains(0))
                                {
                                    hiders[0] = Activator.CreateInstance(configSetType);
                                }

                                object hideDel = Activator.CreateInstance(visiblityDelClassType, __instance, hiders, item);
                                addMethod.Invoke(parentObj, new object[] { Delegate.CreateDelegate(configEvent.EventHandlerType, hideDel, "Run") });

                                break;
                            case ConfigManagerVisibilitySetting.SettingType.ShowIf:
                                if (!showers.Contains(0))
                                {
                                    showers[0] = Activator.CreateInstance(configSetType);
                                }

                                object showDel = Activator.CreateInstance(visiblityDelClassType, __instance, showers, item);
                                addMethod.Invoke(parentObj, new object[] { Delegate.CreateDelegate(configEvent.EventHandlerType, showDel, "Run") });

                                break;
                        }
                    }
                }
            }

            internal class VisibilityDelClass<T, O>
            {
                public VisibilityDelClass(object __instance, IDictionary dict, ConfigManagerVisibilitySetting setting)
                {
                    this.__instance = __instance;
                    this.dict = dict;
                    this.setting = setting;
                }

                readonly object __instance;
                readonly Type dictValType = typeof(HashSet<>).MakeGenericType(configItemType);
                readonly IDictionary dict;
                readonly ConfigManagerVisibilitySetting setting;

                public void Run(T item, O val)
                {
                    string fieldName = AccessTools.Field(configItemType, "fieldName").GetValue(item) as string;
                    if (fieldName == setting.listeningTo)
                    {
                        //Debug.Log($"Changed Field: {fieldName}, New Value: {val}, Satisfied: {setting.IsSatisfied(val)}");
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

        [HarmonyPatch]
        internal static class ConfigItemCreateItemPatch
        {
            private static Type configItemType;
            private static MethodBase configItemCreateItem;

            static bool Prepare(MethodBase original)
            {
                if (original == null)
                {
                    configItemType = Type.GetType("WildfrostHopeMod.Configs.ConfigItem,Config Manager");
                    configItemCreateItem = AccessTools.Method(configItemType, "CreateItem");
                    if (configItemCreateItem != null)
                    {
                        MainModFile.Print($"Config Manager ConfigItem CreateItem patched");
                        return true;
                    }
                    MainModFile.Print($"Config Manager ConfigItem CreateItem not patched");
                    return false;
                }
                return true;
            }

            static MethodBase TargetMethod()
            {
                return configItemCreateItem;
            }

            static void Prefix(object __instance)
            {
                (ConfigItemAttribute attr, FieldInfo fld) = ((ConfigItemAttribute, FieldInfo))AccessTools.Field(configItemType, "con").GetValue(__instance);
                Type containingClass = fld.DeclaringType;
                ConfigManagerCallbackSetting[] callbackSettings = Attribute.GetCustomAttributes(fld, typeof(ConfigManagerCallbackSetting)) as ConfigManagerCallbackSetting[];
                ConfigManagerOptionSetting optionsSetting = Attribute.GetCustomAttribute(fld, typeof(ConfigManagerOptionSetting)) as ConfigManagerOptionSetting;
                Type parentType = AccessTools.Field(configItemType, "parent").FieldType;
                object parentObj = AccessTools.Field(configItemType, "parent").GetValue(__instance);
                PropertyInfo optionProp = AccessTools.Property(configItemType, "optionsAtr");
                Type optionType = optionProp.PropertyType;
                EventInfo configEvent = parentType.GetEvent("OnConfigChanged", AccessTools.all);
                Type callbackDelClassType = typeof(CallbackDelClass<,>).MakeGenericType(configItemType, typeof(object));
                foreach (var item in callbackSettings)
                {
                    MethodInfo callback = AccessTools.Method(containingClass, item.callbackName);
                    object callbackDel = Activator.CreateInstance(callbackDelClassType, __instance, callback);
                    configEvent.AddEventHandler(parentObj, Delegate.CreateDelegate(configEvent.EventHandlerType, callbackDel, "Run"));
                }
                if (optionsSetting != null)
                {
                    optionProp.SetValue(__instance, Activator.CreateInstance(optionType, new object[] { optionsSetting.labels, optionsSetting.vals }));
                }
            }

            internal class CallbackDelClass<T, O>
            {
                public CallbackDelClass(T __instance, MethodInfo callback)
                {
                    this.__instance = __instance;
                    this.callback = callback;
                }
                readonly T __instance;
                readonly MethodInfo callback;

                public void Run(T item, O val)
                {
                    if (__instance.Equals(item))
                    {
                        callback.Invoke(null, new object[] { val });
                    }
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

        /*[HarmonyPatch]
        internal static class CreateButtonLabelPatch
        {
            private static Type configItemType;
            private static MethodBase cbMethod;

            static bool Prepare(MethodBase original)
            {
                if (original == null)
                {
                    configItemType = Type.GetType("WildfrostHopeMod.Configs.ConfigItem,Config Manager");
                    cbMethod = AccessTools.Method(configItemType, "CreateButtonLabel");
                    if (cbMethod != null)
                    {
                        MainModFile.Print($"Config Manager ConfigItem CreateButtonLabel patched");
                        return true;
                    }
                    MainModFile.Print($"Config Manager ConfigItem CreateButtonLabel not patched");
                    return false;
                }
                return true;
            }

            static MethodBase TargetMethod()
            {
                return cbMethod;
            }

            static void Postfix(object __instance, GameObject button)
            {
                (ConfigItemAttribute attr, FieldInfo fld) = ((ConfigItemAttribute, FieldInfo))AccessTools.Field(configItemType, "con").GetValue(__instance);
                ConfigManagerOptionSetting optionsSetting = Attribute.GetCustomAttribute(fld, typeof(ConfigManagerOptionSetting)) as ConfigManagerOptionSetting;
                if (optionsSetting != null)
                {
                    PropertyInfo optionProp = AccessTools.Property(configItemType, "optionsAtr");
                    PropertyInfo currValProp = AccessTools.Property(configItemType, "currentValue");
                    Type optionType = optionProp.PropertyType;
                    var dict = AccessTools.Field(optionType, "lookup").GetValue(optionProp.GetValue(__instance)) as Dictionary<string, object>;
                    object curr = currValProp.GetValue(__instance);
                    string text = "";
                    foreach (var item in dict)
                    {
                        if (item.Value.Equals(curr))
                        {
                            text = item.Key;
                        }
                    }
                    var label = button.transform.FindRecursive("Label");
                    Debug.Log($"Label was made with text: {label.GetComponentInChildren<TextMeshProUGUI>().text}, we want {text}");
                    label.GetComponentInChildren<TextMeshProUGUI>().text = text;
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
        }*/
    }
}
