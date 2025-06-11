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

        // Return false if we should remove
        internal static bool LinkedCheck(StatusIcon __instance)
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
                SpirefrostUtils.FreeReferences(__instance);
            }
        }

        [HarmonyPatch(typeof(StatusIcon), nameof(StatusIcon.CheckRemove))]
        internal static class IconCheckRemovePatch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                List<CodeInstruction> codes = instructions.ToList();
                Label jump = generator.DefineLabel();
                MethodInfo linkCheck = AccessTools.Method(typeof(CustomStatusIconPatches), nameof(LinkedCheck));
                bool jumpInserted = false;
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call, linkCheck);
                yield return new CodeInstruction(OpCodes.Brfalse, jump);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (!jumpInserted && codes[i].opcode == OpCodes.Ldarg_0 && i + 2 < codes.Count && codes[i + 2].opcode == OpCodes.Initobj)
                    {
                        Debug.Log($"IconCheckRemovePatch match found, inserting jump");
                        jumpInserted = true;
                        codes[i].labels.Add(jump);

                    }
                    yield return codes[i];
                }
            }
        }

        [HarmonyPatch(typeof(StatusIcon), nameof(StatusIcon.CheckDestroy))]
        internal static class IconCheckDestroyPatch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                List<CodeInstruction> codes = instructions.ToList();
                Label jump = generator.DefineLabel();
                MethodInfo linkCheck = AccessTools.Method(typeof(CustomStatusIconPatches), nameof(LinkedCheck));
                MethodInfo destroy = AccessTools.Method(typeof(StatusIcon), nameof(StatusIcon.Destroy));
                bool jumpInserted = false;
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call, linkCheck);
                yield return new CodeInstruction(OpCodes.Brfalse, jump);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (!jumpInserted && codes[i].opcode == OpCodes.Ldarg_0 && i + 1 < codes.Count && codes[i + 1].operand as MethodInfo == destroy)
                    {
                        Debug.Log($"IconCheckDestroyPatch match found, inserting jump");
                        jumpInserted = true;
                        codes[i].labels.Add(jump);

                    }
                    yield return codes[i];
                }
            }
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

                        if (data is StatusEffectExtraCounter)
                        {
                            // Move to the left of any Reaction icons
                            Transform parent = statusIcon.transform.parent;
                            if (parent)
                            {
                                foreach (var item in parent.GetAllChildren())
                                {
                                    if (item.GetComponent<StatusIconReaction>())
                                    {
                                        statusIcon.transform.SetSiblingIndex(item.GetSiblingIndex());
                                        break;
                                    }
                                }
                            }
                        }

                        Debug.Log($"DoCustomIcons - Linked Status {data} to Icon");
                        statusIcon.LinkStatus(data);

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
        }
    }
}
