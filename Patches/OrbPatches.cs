using HarmonyLib;
using MonoMod.Utils;
using Spirefrost.StatusEffects;
using System;
using System.Collections.Generic;
using System.Reflection;
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

    [HarmonyPatch(typeof(InspectSystem), "CreateIconPopups")]
    internal static class OrbPopups
    {
        static void Postfix(InspectSystem __instance, RectTransform iconLayoutGroup, Transform popGroup)
        {
            if (iconLayoutGroup == __instance.inspect.display.damageLayoutGroup && popGroup == __instance.rightPopGroup && __instance.inspect.display is Card)
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
}
