using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using static Deadpan.Enums.Engine.Components.Modding.WildfrostMod;
using static Spirefrost.Patches.ConfigPatches.ConfigItemCreateItemPatch;

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
        internal class ConfigManagerExclusiveSetting : Attribute
        {
            internal ConfigManagerExclusiveSetting(string exclusiveWith, object blockingValue, object blockingOutput)
            {
                this.exclusiveWith = exclusiveWith;
                blockingMap = new Dictionary<object, object>
                {
                    [blockingValue] = blockingOutput
                };
            }

            internal ConfigManagerExclusiveSetting(string exclusiveWith, object[] blockingValues, object blockingOutput)
            {
                this.exclusiveWith = exclusiveWith;
                blockingMap = new Dictionary<object, object>();
                for (int i = 0; i < blockingValues.Length; i++)
                {
                    blockingMap[blockingValues[i]] = blockingOutput;
                }
            }

            internal ConfigManagerExclusiveSetting(string exclusiveWith, object[] blockingValues, object[] blockingOutputs)
            {
                this.exclusiveWith = exclusiveWith;
                blockingMap = new Dictionary<object, object>();
                int outputMax = blockingOutputs.Length - 1;
                for (int i = 0; i < blockingValues.Length; i++)
                {
                    blockingMap[blockingValues[i]] = blockingOutputs[Math.Min(i, outputMax)];
                }
            }

            internal string exclusiveWith;
            internal Dictionary<object, object> blockingMap;

            internal bool IsSatisfied(object value)
            {
                return blockingMap.ContainsKey(value);
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
            private static FieldInfo conField;
            private static FieldInfo parentField;
            private static Type parentType;
            private static EventInfo configEvent;
            private static MethodInfo addMethod;
            private static Type configSetType;
            private static Type visiblityDelClassType;
            private static FieldInfo itemsField;
            private static FieldInfo hidersField;
            private static FieldInfo showersField;

            static bool Prepare(MethodBase original)
            {
                if (original == null)
                {
                    configItemType = Type.GetType("WildfrostHopeMod.Configs.ConfigItem,Config Manager");
                    if (configItemType != null)
                    {
                        try
                        {
                            configItemSVL = AccessTools.Method(configItemType, "SetVisibilityListeners");
                            conField = AccessTools.Field(configItemType, "con");
                            parentField = AccessTools.Field(configItemType, "parent");
                            parentType = parentField.FieldType;
                            configEvent = parentType.GetEvent("OnConfigChanged", AccessTools.all);
                            addMethod = configEvent.AddMethod;
                            configSetType = typeof(HashSet<>).MakeGenericType(configItemType);
                            visiblityDelClassType = typeof(VisibilityDelClass<,>).MakeGenericType(configItemType, typeof(object));
                            itemsField = AccessTools.Field(parentType, "items");
                            hidersField = AccessTools.Field(configItemType, "hiders");
                            showersField = AccessTools.Field(configItemType, "showers");
                            List<FieldInfo> nullFields = AccessTools.GetDeclaredFields(typeof(SetVisibilityListenersPatch)).Where(fi => fi.GetValue(null) == null).ToList();
                            if (nullFields.Count > 0)
                            {
                                MainModFile.Print($"Config Manager ConfigItem.SetVisibilityListeners not patched, got null fields:");
                                MainModFile.Print(nullFields.Join());
                                return false;
                            }
                            MainModFile.Print($"Config Manager ConfigItem.SetVisibilityListeners patched");
                            return true;
                        }
                        catch (Exception e)
                        {
                            MainModFile.Print($"Patching Config Manager Failed:");
                            Debug.Log(e);
                        }
                    }
                    MainModFile.Print($"Config Manager ConfigItem.SetVisibilityListeners not patched, type not found");
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
                (ConfigItemAttribute attr, FieldInfo fld) = ((ConfigItemAttribute, FieldInfo))conField.GetValue(__instance);
                ConfigManagerVisibilitySetting[] settings = Attribute.GetCustomAttributes(fld, typeof(ConfigManagerVisibilitySetting)) as ConfigManagerVisibilitySetting[];
                string fieldName = fld.Name;
                object parentObj = parentField.GetValue(__instance);
                IDictionary items = itemsField.GetValue(parentObj) as IDictionary;
                IDictionary hiders = hidersField.GetValue(__instance) as IDictionary;
                IDictionary showers = showersField.GetValue(__instance) as IDictionary;
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
            private static FieldInfo conField;
            private static FieldInfo parentField;
            private static Type parentType;
            private static FieldInfo itemsField;
            private static PropertyInfo optionProp;
            private static Type optionType;
            private static EventInfo configEvent;
            private static MethodInfo invokeChange;
            private static MethodInfo updateLabel;
            private static PropertyInfo currentProp;
            private static FieldInfo buttonField;

            static bool Prepare(MethodBase original)
            {
                if (original == null)
                {
                    configItemType = Type.GetType("WildfrostHopeMod.Configs.ConfigItem,Config Manager");
                    if (configItemType != null)
                    {
                        try 
                        {
                            configItemCreateItem = AccessTools.Method(configItemType, "CreateItem");
                            conField = AccessTools.Field(configItemType, "con");
                            parentField = AccessTools.Field(configItemType, "parent");
                            parentType = parentField.FieldType;
                            itemsField = AccessTools.Field(parentType, "items");
                            optionProp = AccessTools.Property(configItemType, "optionsAtr");
                            optionType = optionProp.PropertyType;
                            configEvent = parentType.GetEvent("OnConfigChanged", AccessTools.all);
                            invokeChange = AccessTools.Method(parentType, "InvokeConfigChanged");
                            updateLabel = AccessTools.Method(configItemType, "UpdateLabel");
                            currentProp = AccessTools.Property(configItemType, "currentValue");
                            buttonField = AccessTools.Field(configItemType, "button");
                            List<FieldInfo> nullFields = AccessTools.GetDeclaredFields(typeof(ConfigItemCreateItemPatch)).Where(fi => fi.GetValue(null) == null).ToList();
                            if (nullFields.Count > 0)
                            {
                                MainModFile.Print($"Config Manager ConfigItem.CreateItem not patched, got null fields:");
                                MainModFile.Print(nullFields.Join());
                                return false;
                            }
                            MainModFile.Print($"Config Manager ConfigItem.CreateItem patched");
                            return true;
                        } catch (Exception e)
                        {
                            MainModFile.Print($"Patching Config Manager Failed:");
                            Debug.Log(e);
                        }
                    }
                    MainModFile.Print($"Config Manager ConfigItem.CreateItem not patched, type not found");
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
                (ConfigItemAttribute attr, FieldInfo fld) = ((ConfigItemAttribute, FieldInfo))conField.GetValue(__instance);
                Type containingClass = fld.DeclaringType;
                ConfigManagerCallbackSetting[] callbackSettings = Attribute.GetCustomAttributes(fld, typeof(ConfigManagerCallbackSetting)) as ConfigManagerCallbackSetting[];
                ConfigManagerExclusiveSetting[] exclusiveSettings = Attribute.GetCustomAttributes(fld, typeof(ConfigManagerExclusiveSetting)) as ConfigManagerExclusiveSetting[];
                ConfigManagerOptionSetting optionsSetting = Attribute.GetCustomAttribute(fld, typeof(ConfigManagerOptionSetting)) as ConfigManagerOptionSetting;
                
                object parentObj = parentField.GetValue(__instance);
                IDictionary items = itemsField.GetValue(parentObj) as IDictionary;
                Type callbackDelClassType = typeof(CallbackDelClass<,>).MakeGenericType(configItemType, typeof(object));
                Type exclusiveDelClassType = typeof(ExclusiveDelClass<,>).MakeGenericType(configItemType, typeof(object));

                foreach (var item in callbackSettings)
                {
                    MethodInfo callback = AccessTools.Method(containingClass, item.callbackName);
                    object callbackDel = Activator.CreateInstance(callbackDelClassType, __instance, callback);
                    configEvent.AddEventHandler(parentObj, Delegate.CreateDelegate(configEvent.EventHandlerType, callbackDel, "Run"));
                }
                foreach (var item in exclusiveSettings)
                {
                    object exclusiveDel = Activator.CreateInstance(exclusiveDelClassType, __instance, item);
                    configEvent.AddEventHandler(parentObj, Delegate.CreateDelegate(configEvent.EventHandlerType, exclusiveDel, "Run"));
                }
                if (optionsSetting != null)
                {
                    optionProp.SetValue(__instance, Activator.CreateInstance(optionType, new object[] { optionsSetting.labels, optionsSetting.vals }));
                }
            }

            internal class ExclusiveDelClass<T, O>
            {
                public ExclusiveDelClass(T __instance, ConfigManagerExclusiveSetting setting)
                {
                    this.__instance = __instance;
                    this.invokeTarget = parentField.GetValue(__instance);
                    this.lookup = itemsField.GetValue(invokeTarget) as IDictionary;
                    this.setting = setting;
                }

                readonly T __instance;
                readonly object invokeTarget;
                readonly IDictionary lookup;
                readonly ConfigManagerExclusiveSetting setting;

                public void Run(T item, O val)
                {
                    if (__instance.Equals(item) && setting.blockingMap.ContainsKey(val))
                    {
                        object exclusive = lookup[setting.exclusiveWith];
                        object blockedVal = setting.blockingMap[val];
                        Debug.Log($"Value before invoke {currentProp.GetValue(exclusive)}");
                        invokeChange.Invoke(invokeTarget, new object[] { exclusive, blockedVal });
                        Debug.Log($"Value after invoke {currentProp.GetValue(exclusive)}");
                        updateLabel.Invoke(exclusive, null);
                        GameObject button = buttonField.GetValue(exclusive) as GameObject;
                        var settingOptions = button.GetComponent<SettingOptions>();
                        int index = 0;
                        if (blockedVal.GetType().IsEnum)
                        {
                            index = (int)blockedVal;
                        }
                        else if (blockedVal is bool b)
                        {
                            index = b ? 1 : 0;
                        }
                        settingOptions.SetValue(index);
                        settingOptions.onValueChanged?.Invoke(settingOptions.dropdown.value);
                    }
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
    }
}
