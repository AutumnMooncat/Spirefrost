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
                            statusIcon.persistent = false;
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
