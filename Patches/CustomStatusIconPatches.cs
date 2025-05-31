using HarmonyLib;
using Spirefrost.StatusEffects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Spirefrost.Patches
{
    [HarmonyPatch]
    internal class CustomStatusIconPatches
    {
        internal static string HasLinkedKey => "hasLinkedData";
        internal static string LinkedKey => "linkedData";

        [HarmonyPatch]
        internal static class CustomLogicInsert
        {
            private static Type createdType;
            public static MethodBase TargetMethod()
            {
                return SpirefrostUtils.FindEnumeratorMethod(AccessTools.DeclaredMethod(typeof(EntityDisplay), nameof(EntityDisplay.UpdateDisplay)), ref createdType);
            }

            // Return if we should do the original code
            private static bool CustomIconCheck(StatusEffectData data)
            {
                if (data is INonStackingStatusEffect)
                {
                    return false;
                }
                return true;
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

                FieldInfo iconsField = AccessTools.Field(typeof(EntityDisplay), nameof(EntityDisplay.iconGroups));
                FieldInfo entityField = AccessTools.Field(typeof(EntityDisplay), nameof(EntityDisplay.entity));
                FieldInfo pingField = AccessTools.Field(createdType, "doPing");
                FieldInfo visibleField = AccessTools.Field(typeof(StatusEffectData), nameof(StatusEffectData.visible));

                MethodInfo customCheck = AccessTools.Method(typeof(CustomLogicInsert), nameof(CustomIconCheck));
                MethodInfo doCustomIcons = AccessTools.Method(typeof(CustomIconLogic), nameof(CustomIconLogic.DoCustomIcons));

                bool checkInserted = false;
                bool doInserted = false;

                for (int i = 0; i < codes.Count; i++)
                {
                    if (!doInserted && codes[i].opcode == OpCodes.Ldfld && codes[i].operand as FieldInfo == iconsField)
                    {
                        Debug.Log("CustomLogicInsert - Match found, inserting handler");
                        doInserted = true;
                        // Ldloc_1 already on the stack, we load from it
                        yield return new CodeInstruction(OpCodes.Ldfld, entityField);
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, pingField);
                        yield return new CodeInstruction(OpCodes.Call, doCustomIcons);
                        // Put Ldloc_1 back
                        yield return new CodeInstruction(OpCodes.Ldloc_1);
                    }
                    yield return codes[i];
                    if (!checkInserted && codes[i].opcode == OpCodes.Ldfld && codes[i].operand as FieldInfo == visibleField)
                    {
                        Debug.Log("CustomLogicInsert - Match found, inserting check");
                        checkInserted = true;
                        // bool already on the stack, will break on false
                        yield return new CodeInstruction(OpCodes.Ldloc_3);
                        yield return new CodeInstruction(OpCodes.Call, customCheck);
                        yield return new CodeInstruction(OpCodes.And);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(StatusIcon), nameof(StatusIcon.Destroy))]
        internal static class IconDestroyPatch
        {
            static void Prefix(StatusIcon __instance)
            {
                SpirefrostUtils.FreeReference(__instance);
            }
        }

        [HarmonyPatch(typeof(StatusIcon), nameof(StatusIcon.CheckRemove))]
        internal static class IconCheckRemovePatch
        {
            // Return false if we should remove
            static bool LinkedCheck(StatusIcon __instance)
            {

                if (__instance.HasLinkedStatus())
                {
                    StatusEffectData linked = __instance.GetLinkedStatus();
                    if (linked == null || !__instance.target.statusEffects.Contains(linked))
                    {
                        MainModFile.Print($"Linked Status not found, we should remove");
                        return false;
                    }
                }
                return true;
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> codes = instructions.ToList();
                MethodInfo opImp = AccessTools.Method(typeof(UnityEngine.Object), "op_Implicit");
                MethodInfo linkCheck = AccessTools.Method(typeof(IconCheckRemovePatch), nameof(LinkedCheck));
                bool checkInserted = false;
                for (int i = 0; i < codes.Count; i++)
                {
                    yield return codes[i];
                    if (!checkInserted && codes[i].opcode == OpCodes.Call && codes[i].operand as MethodInfo == opImp)
                    {
                        Debug.Log($"IconCheckRemovePatch match found, inserted check");
                        checkInserted = true;
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, linkCheck);
                        yield return new CodeInstruction(OpCodes.And);
                    }
                }
            }
        }

        [HarmonyPatch]
        internal static class CounterImminentPatches
        {
            internal static List<StatusIconCounter> GetCustomCounterIcons(Entity entity)
            {
                return entity.statusEffects
                    .Select(effect => effect as INonStackingStatusEffect)
                    .Where(effect => effect != null && effect.Icon is StatusIconCounter counter)
                    .Select(effect => effect.Icon as StatusIconCounter)
                    .ToList();
            }

            internal static bool CounterCheck(bool enable, Entity entity)
            {
                return enable && entity.counter.current == 1;
            }

            [HarmonyPatch(typeof(CounterImminentDisplaySystem), nameof(CounterImminentDisplaySystem.Imminent))]
            internal static class ImminentPatch
            {
                static void Postfix(Entity entity, ref bool __result)
                {
                    if (!entity.IsSnowed && GetCustomCounterIcons(entity).Any(icon => icon.GetValue().current == 1)) 
                    {
                        __result = true;
                    }
                }
            }

            [HarmonyPatch(typeof(CounterImminentDisplaySystem), nameof(CounterImminentDisplaySystem.SetCounterIconAnimation))]
            internal static class SetCounterIconAnimationPatch
            {
                static void Postfix(Entity entity, bool enable)
                {
                    foreach (var item in GetCustomCounterIcons(entity))
                    {
                        CardIdleAnimation imminentAnimation = item.imminentAnimation;
                        if (imminentAnimation != null)
                        {
                            if (enable && item.GetValue().current == 1)
                            {
                                imminentAnimation.FadeIn();
                                continue;
                            }
                            imminentAnimation.FadeOut();
                        }
                    }
                }

                static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    List<CodeInstruction> codes = instructions.ToList();
                    MethodInfo check = AccessTools.Method(typeof(CounterImminentPatches), nameof(CounterCheck));

                    for (int i = 0; i < codes.Count; i++)
                    {
                        yield return codes[i];

                        if (codes[i].opcode == OpCodes.Ldarg_2)
                        {
                            Debug.Log($"SetCounterIconAnimationPatch found 'enable' load, inserting check");
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Call, check);
                        }
                    }
                }
            }
        }

        internal class CustomStatusIcon : StatusIcon
        {
            public StatusEffectData linkedData;

            /*public override void CheckRemove()
            {
                if (linkedData == null || (!persistent && !target.statusEffects.Contains(linkedData)))
                {
                    MainModFile.Print($"Status not found, we should remove");
                    SetValue(default);
                    Destroy();
                }
            }*/
        }

        internal class CustomIconLogic
        {
            internal static void DoCustomIcons(Entity entity, bool doPing)
            {
                //Debug.Log($"DoCustomIcons - Begin");
                foreach (StatusEffectData statusEffect in entity.statusEffects)
                {
                    if (statusEffect.visible && !statusEffect.iconGroupName.IsNullOrEmpty() && statusEffect is INonStackingStatusEffect)
                    {
                        SetCustomIcon(entity, entity.display, statusEffect, doPing);
                    }
                }
            }

            private static void SetCustomIcon(Entity entity, EntityDisplay display, StatusEffectData data, bool doPing)
            {
                //Debug.Log($"DoCustomIcons - Entity {entity}, Status {data}");
                StatusIcon statusIcon = FindCustomIcon(display, data);
                if ((bool)statusIcon)
                {
                    UpdateCustomIcon(statusIcon, new Stat(data.count, 0), doPing);
                }
                else
                {
                    statusIcon = CardManager.NewStatusIcon(data.type, display.iconGroups[data.iconGroupName]);
                    if (!statusIcon)
                    {
                        Debug.LogError("Status Icon for [" + data.type + "] NOT FOUND!");
                    }
                    else
                    {
                        //Debug.Log($"DoCustomIcons - Created Icon {statusIcon}");
                        if (data is INonStackingStatusEffect nonStacking)
                        {
                            nonStacking.Icon = statusIcon;
                        }
                        Debug.Log($"DoCustomIcons - Linked Status {data} to Icon");
                        statusIcon.LinkStatus(data);
                        
                        /*if (statusIcon is CustomStatusIcon custom)
                        {
                            Debug.Log($"DoCustomIcons - Linked Status {data} to Icon");
                            custom.linkedData = data;
                        }
                        else if (statusIcon is CustomCounterIcon customCounter)
                        {
                            Debug.Log($"DoCustomIcons - Linked Status {data} to Icon");
                            customCounter.linkedData = data;
                        }*/

                        if (display.hover)
                        {
                            CardHover component = statusIcon.GetComponent<CardHover>();
                            component.master = display.hover;
                            component.enabled = true;
                        }

                        statusIcon.Assign(entity);
                        statusIcon.SetValue(new Stat(data.count, 0), doPing);
                        statusIcon.SetText();
                        if (doPing)
                        {
                            statusIcon.CreateEvent();
                            Events.InvokeStatusIconCreated(statusIcon);
                        }
                    }
                }
            }

            private static StatusIcon FindCustomIcon(EntityDisplay display, StatusEffectData data)
            {
                //Debug.Log($"DoCustomIcons - Finding Icon for Status {data}");
                StatusIcon statusIcon = null;
                foreach (var item in display.iconGroups)
                {
                    foreach (RectTransform rect in item.Value)
                    {
                        StatusIcon icon = rect.GetComponent<StatusIcon>();
                        if (icon && icon.HasLinkedStatus() && icon.GetLinkedStatus() == data)
                        {
                            statusIcon = icon;
                            break;
                        }
                        CustomStatusIcon custom = rect.GetComponent<CustomStatusIcon>();
                        if (custom && custom.linkedData == data)
                        {
                            statusIcon = custom;
                            break;
                        }
                    }
                }

                //Debug.Log($"DoCustomIcons - Returning Icon {statusIcon}");
                return statusIcon;
            }

            private static void UpdateCustomIcon(StatusIcon icon, Stat value, bool doPing)
            {
                //Debug.Log($"DoCustomIcons - Update Custom {icon}, Value {value.current}");
                icon.SetValue(value, doPing);
                //Debug.Log($"Has textElement? {icon.textElement != null}");
                icon.SetText();
            }

            /*private static StatusIcon MakeCustomIcon(StatusIcon icon)
            {
                Debug.Log($"Making custom counter icon from {icon}");
                GameObject gameObject = icon.gameObject;
                StatusIcon thingy;
                if (icon is StatusIconCounter counter)
                {
                    thingy = gameObject.GetOrAdd<CustomCounterIcon>();
                    foreach (var item in AccessTools.GetDeclaredFields(typeof(StatusIconCounter)))
                    {
                        Debug.Log($"Copying {item.Name} -> {item.GetValue(counter)}");
                        item.SetValue(thingy, item.GetValue(counter));
                    }
                }
                else
                {
                    thingy = gameObject.GetOrAdd<CustomStatusIcon>();
                }
                foreach (var item in AccessTools.GetDeclaredFields(typeof(StatusIcon)))
                {
                    Debug.Log($"Copying {item.Name} -> {item.GetValue(icon)}");
                    item.SetValue(thingy, item.GetValue(icon));
                }
                GameObject.Destroy(icon);
                Rereference(thingy);
                return thingy;
            }

            private static void Rereference(StatusIcon icon)
            {
                icon.afterUpdate = new UnityEngine.Events.UnityEvent();
                Debug.Log($"Adding SetText listener");
                icon.afterUpdate.AddListener(icon.SetText);
                if (icon is CustomCounterIcon counter)
                {
                    Debug.Log($"Adding CheckSetSprite listener");
                    icon.afterUpdate.AddListener(counter.CheckSetSprite);
                }
            }*/

            /*private static void Rereference(StatusIcon newIcon, object current, Type currentType, Dictionary<Type, List<object>> processed, bool root = false)
            {
                Debug.Log($"Processing {current}({currentType})");
                if (currentType.IsValueType)
                {
                    Debug.Log($"Skipping value type");
                    return;
                }
                if (!processed.ContainsKey(currentType))
                {
                    processed[currentType] = new List<object>();
                }
                if (processed[currentType].Contains(current))
                {
                    Debug.Log($"Already processed, skipping");
                    return;
                }
                processed[currentType].Add(current);
                Dictionary<Type, List<object>> foundMap = new Dictionary<Type, List<object>>();
                foreach (var item in currentType.GetFields(AccessTools.all))
                {
                    Debug.Log($"Found {currentType} Field {item.Name}({item.FieldType})");
                    if (item.FieldType.IsAssignableFrom(typeof(StatusIcon)))
                    {
                        if (item.IsInitOnly)
                        {
                            Debug.Log($"Cant Rereference {item.Name}, its readonly");
                            continue;
                        }
                        Debug.Log($"Rereferencing {item.Name}");
                        item.SetValue(current, newIcon);
                        continue;
                    }
                    if (item.FieldType.IsValueType)
                    {
                        Debug.Log($"Skipping value type");
                        continue;
                    }
                    object found = item.GetValue(current);
                    if (found == null)
                    {
                        Debug.Log($"Field is null");
                        continue;
                    }
                    Debug.Log($"Got {found}");
                    if (!foundMap.ContainsKey(item.FieldType))
                    {
                        foundMap[item.FieldType] = new List<object>();
                    }
                    if (!foundMap[item.FieldType].Contains(found))
                    {
                        foundMap[item.FieldType].Add(found);
                    }
                }

                foreach (var item in currentType.GetProperties(AccessTools.all))
                {
                    Debug.Log($"Found {currentType} Property {item.Name}({item.PropertyType})");
                    if (item.PropertyType.IsAssignableFrom(typeof(StatusIcon)))
                    {
                        if (item.SetMethod == null)
                        {
                            Debug.Log($"Cant Rereference {item.Name}, no set method");
                            continue;
                        }
                        Debug.Log($"Rereferencing {item.Name}");
                        item.SetValue(current, newIcon);
                        continue;
                    }
                    if (item.PropertyType.IsValueType)
                    {
                        Debug.Log($"Skipping value type");
                        continue;
                    }
                    object found = item.GetValue(current);
                    if (found == null)
                    {
                        Debug.Log($"Property is null");
                        continue;
                    }
                    Debug.Log($"Got {found}");
                    if (!foundMap.ContainsKey(item.PropertyType))
                    {
                        foundMap[item.PropertyType] = new List<object>();
                    }
                    if (!foundMap[item.PropertyType].Contains(found))
                    {
                        foundMap[item.PropertyType].Add(found);
                    }
                }

                Debug.Log($"Found {foundMap.Aggregate(0, (total, pair) => total += pair.Value.Count)} things to check");
                foreach (var item in foundMap)
                {
                    foreach (var found in item.Value)
                    {
                        Debug.Log($"Checking {found}({item.Key})");
                        if (item.Key.IsGenericType && item.Key.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            Debug.Log($"This is a list, iterating");
                            IList list = found as IList;
                            Debug.Log($"Found {list?.Count ?? 0} items");
                            foreach (var obj in list)
                            {
                                Rereference(newIcon, obj, item.Key.GenericTypeArguments[0], processed);
                            }
                        }
                        else if (typeof(Delegate).IsAssignableFrom(item.Key))
                        {
                            Debug.Log($"This is an action, finding events");
                            Delegate action = found as Delegate;
                            foreach (var del in action.GetInvocationList())
                            {
                                Debug.Log($"Found {del.Method.Name} from {del.Method.DeclaringType}");
                                Debug.Log($"");
                                FieldInfo target = AccessTools.Field(typeof(Delegate), "_target");
                                Type targetType = target.FieldType;
                                Debug.Log($"Its type is {targetType}");
                                if (targetType.IsAssignableFrom(typeof(StatusIcon)))
                                {
                                    Debug.Log($"Rereferencing {del.Method.Name}");
                                    target.SetValue(del, newIcon);
                                }
                            }
                        }
                        else
                        {
                            Rereference(newIcon, found, item.Key, processed);
                        }
                    }
                }

                if (root)
                {
                    Debug.Log($"Walking up event base");
                    Rereference(newIcon, current, typeof(UnityEventBase), processed);
                }
                else if (currentType.BaseType != null)
                {
                    Debug.Log($"Walking up inheritence");
                    Rereference(newIcon, current, currentType.BaseType, processed);
                }
            }*/
        }
    }
}
