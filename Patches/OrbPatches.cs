using HarmonyLib;
using MonoMod.Utils;
using Spirefrost.StatusEffects;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;

namespace Spirefrost.Patches
{
    [HarmonyPatch(typeof(EntityDisplay), "Awake")]
    internal static class LayoutPatch
    {
        internal static string orbIconGroup = "stsorbs";
        static void Postfix(EntityDisplay __instance)
        {
            if (__instance is Card)
            {
                // Create and align layout
                GameObject layout = new GameObject("OrbLayout", typeof(RectTransform), typeof(CircularLayoutGroup), typeof(ContentSizeFitter));
                layout.transform.SetParent(__instance.GetCanvas().transform.GetChild(1), false);
                layout.transform.SetLocalY(layout.transform.localPosition.y + 0.75f);
                RectTransform rect = layout.GetComponent<RectTransform>();
                rect.Align(layout.transform);

                // Adjust group
                CircularLayoutGroup group = layout.GetComponent<CircularLayoutGroup>();
                group.spacing = -0.3f;
                group.radius = 0.5f;
                group.childAlignment = TextAnchor.MiddleCenter;

                // Adjust fitter
                ContentSizeFitter fitter = layout.GetComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

                // Add to dictionary
                __instance.iconGroups[orbIconGroup] = rect;
            }
        }
    }

    [HarmonyPatch(typeof(EntityDisplay), "SetStatusIcon")]
    internal static class IgnoreOrbs
    {
        static bool Prefix(EntityDisplay __instance, string iconGroupName)
        {
            if (iconGroupName == LayoutPatch.orbIconGroup)
            {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    internal static class CustomLogicInsert
    {
        private static Type createdType;
        public static MethodBase TargetMethod()
        {
            return SpirefrostUtils.FindEnumeratorMethod(AccessTools.DeclaredMethod(typeof(EntityDisplay), "UpdateDisplay"), ref createdType);
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo iconsField = typeof(EntityDisplay).GetField("iconGroups", AccessTools.all);
            
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldfld)
                {
                    if (codes[i].operand is FieldInfo info && info == iconsField)
                    {
                        Debug.Log("CustomLogicInsert - Match found, injecting new instructions");
                        // Ldloc_1 already on the stack, we load from it
                        yield return new CodeInstruction(OpCodes.Ldfld, typeof(EntityDisplay).GetField("entity", AccessTools.all));
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, createdType.GetField("doPing"));
                        yield return new CodeInstruction(OpCodes.Call, typeof(CustomIconLogic).GetMethod("DoCustomIcons", BindingFlags.Static | BindingFlags.NonPublic));
                        // Put Ldloc_1 back
                        yield return new CodeInstruction(OpCodes.Ldloc_1);
                    }
                }
                yield return codes[i];
            }
        }
    }

    [HarmonyPatch(typeof(InspectSystem), "CreateIconPopups")]
    internal static class OrbPopups
    {
        static void Postfix(InspectSystem __instance, RectTransform iconLayoutGroup, Transform popGroup)
        {
            if (iconLayoutGroup == __instance.inspect.display.damageLayoutGroup && popGroup == __instance.rightPopGroup)
            {
                CardPopUpTarget[] componentsInChildren = __instance.inspect.display.iconGroups[LayoutPatch.orbIconGroup].GetComponentsInChildren<CardPopUpTarget>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    KeywordData[] keywords = componentsInChildren[i].keywords;
                    foreach (KeywordData keyword in keywords)
                    {
                        __instance.Popup(keyword, popGroup);
                    }
                }
            }
        }
    }

    [HarmonyPatch]
    internal static class StopRemovingMyStuff
    {
        private static readonly List<MethodInfo> methods = new List<MethodInfo>();
        public static bool Prepare(MethodBase original)
        {
            if (original == null)
            {
                Type statusPredicate = Type.GetType("WildfrostHopeMod.CommandsConsole.ConsoleCustom+CommandAddStatus+<>c,Commands Console");
                MethodInfo statusPredicateMethod = statusPredicate?.FindMethod("<Routine>b__6_2");
                if (statusPredicateMethod != null)
                {
                    MainModFile.Print($"Patching add status command");
                    methods.Add(statusPredicateMethod);
                }
                Type effectPredicate = Type.GetType("WildfrostHopeMod.CommandsConsole.ConsoleCustom+CommandAddEffect+<>c,Commands Console");
                MethodInfo effectPredicateMethod = effectPredicate?.FindMethod("<Routine>b__6_2");
                if (effectPredicateMethod != null)
                {
                    MainModFile.Print($"Patching add effect command");
                    methods.Add(effectPredicateMethod);
                }
                if (methods.Count == 0)
                {
                    MainModFile.Print($"Commands Console not patched");
                }
                return methods.Count > 0;
            }
            return true;
        }

        public static IEnumerable<MethodBase> TargetMethods()
        {
            return methods;
        }

        public static void Postfix(StatusEffectData s, ref bool __result)
        {
            if (s is StatusEffectOrb)
            {
                __result = false;
            }
        }

        public static Exception Cleanup(MethodBase original, Exception exception)
        {
            if (exception != null)
            {
                MainModFile.Print($"Patching Commands Console Failed:");
                Debug.Log(exception);
            }
            return null;
        }
    }

    internal class CircularLayoutGroup : LayoutGroup
    {
        public float spacing;

        public float radius;

        private float ToRadians(float degrees)
        {
            return degrees * (float)Math.PI / 180;
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalculateLayoutInputCircular();
        }

        public override void CalculateLayoutInputVertical()
        {
            CalculateLayoutInputCircular();
        }

        private void CalculateLayoutInputCircular()
        {
            int axis = 0;
            float padding = ((axis == 0) ? base.padding.horizontal : base.padding.vertical);
            bool controlSize = false;
            bool childForceExpand = true;
            float totalMin = padding;
            float totalPreferred = padding;
            float totalFlexible = 0f;
            int count = base.rectChildren.Count;
            for (int i = 0; i < count; i++)
            {
                RectTransform rectTransform = base.rectChildren[i];
                GetChildSizes(rectTransform, axis, controlSize, childForceExpand, out var min, out var preferred, out var flexible);

                totalMin += min + spacing;
                totalPreferred += preferred + spacing;
                totalFlexible += flexible;
            }

            if (base.rectChildren.Count > 0)
            {
                totalMin -= spacing;
                totalPreferred -= spacing;
            }

            totalPreferred = Mathf.Max(totalMin, totalPreferred);
            SetLayoutInputForAxis(totalMin, totalPreferred, totalFlexible, axis);
        }

        public override void SetLayoutHorizontal()
        {
            SetLayoutCircular();
        }

        public override void SetLayoutVertical()
        {
            SetLayoutCircular();
        }

        private void SetLayoutCircular()
        {
            int count = base.rectChildren.Count;
            int axis = 0;
            float layoutSize = base.rectTransform.rect.size[axis];
            float layoutSizeMinusPadding = layoutSize - (float)((axis == 0) ? base.padding.horizontal : base.padding.vertical);

            if (count == 1)
            {
                // Center it
                RectTransform rectTransform = base.rectChildren[0];
                GetChildSizes(rectTransform, axis, false, true, out var min, out _, out _);
                float scale = 1f;
                float paddingClamp = Mathf.Clamp(layoutSizeMinusPadding, min, layoutSize);
                float startOffset = GetStartOffset(axis, paddingClamp * scale);
                float offset = (paddingClamp - rectTransform.sizeDelta[axis]) * GetAlignmentOnAxis(axis);
                SetChildAlongAxisWithScale(rectTransform, axis, startOffset + offset, scale);
            }
            else
            {
                // Circle Time
                float rotationDeg = count == 2 ? 90 : 0;
                float rotationDegDelta = 360f / count;
                for (int i = 0; i < count; i += 1)
                {
                    float dx = (float)(radius * Math.Sin(ToRadians(rotationDeg)));
                    float dy = (float)(radius * Math.Cos(ToRadians(rotationDeg)));
                    RectTransform rectTransform = base.rectChildren[i];
                    GetChildSizes(rectTransform, axis, false, true, out var min, out _, out _);
                    float scale = 1f;
                    float paddingClamp = Mathf.Clamp(layoutSizeMinusPadding, min, layoutSize);
                    float startOffsetX = GetStartOffset(0, paddingClamp * scale);
                    float offsetX = (paddingClamp - rectTransform.sizeDelta[0]) * GetAlignmentOnAxis(0);
                    float startOffsetY = GetStartOffset(1, paddingClamp * scale);
                    float offsetY = (paddingClamp - rectTransform.sizeDelta[1]) * GetAlignmentOnAxis(1);
                    SetChildAlongAxisWithScale(rectTransform, 0, startOffsetX + offsetX + dx, scale);
                    SetChildAlongAxisWithScale(rectTransform, 1, startOffsetY + offsetY + dy, scale);
                    rotationDeg += rotationDegDelta;
                }
            }
        }

        private void GetChildSizes(RectTransform child, int axis, bool controlSize, bool childForceExpand, out float min, out float preferred, out float flexible)
        {
            if (!controlSize)
            {
                min = child.sizeDelta[axis];
                preferred = min;
                flexible = 0f;
            }
            else
            {
                min = LayoutUtility.GetMinSize(child, axis);
                preferred = LayoutUtility.GetPreferredSize(child, axis);
                flexible = LayoutUtility.GetFlexibleSize(child, axis);
            }

            if (childForceExpand)
            {
                flexible = Mathf.Max(flexible, 1f);
            }
        }
    }

    internal class CustomStatusIcon : StatusIcon
    {
        public StatusEffectData linkedData;

        public override void CheckRemove()
        {
            if (linkedData == null || (!persistent && !target.statusEffects.Contains(linkedData)))
            {
                MainModFile.Print($"Status not found, we should remove");
                SetValue(default);
                Destroy();
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
                if (statusEffect.visible && statusEffect.iconGroupName == LayoutPatch.orbIconGroup)
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
                statusIcon = CardManager.NewStatusIcon(data.type, display.iconGroups[LayoutPatch.orbIconGroup]);
                if (!statusIcon)
                {
                    Debug.LogError("Status Icon for [" + data.type + "] NOT FOUND!");
                }
                else
                {
                    //Debug.Log($"DoCustomIcons - Created Icon {statusIcon}");
                    if (statusIcon is CustomStatusIcon custom)
                    {
                        Debug.Log($"DoCustomIcons - Linked Status {data} to Icon");
                        custom.linkedData = data;
                    }

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
            foreach (RectTransform item in display.iconGroups[LayoutPatch.orbIconGroup])
            {
                CustomStatusIcon component = item.GetComponent<CustomStatusIcon>();
                if (component && component.linkedData == data)
                {
                    statusIcon = component;
                    break;
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
