using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Spirefrost.Patches
{
    [HarmonyPatch(typeof(StatusIcon), "CheckRemove")]
    internal static class DebugTest
    {
        //internal static List<StatusIcon> destroyedIcons = new List<StatusIcon>();
        static bool Prefix(StatusIcon __instance)
        {
            MainModFile.Print($"Checking remove for {__instance}, its type is {__instance.type}, its target is {__instance.target}");
            //MainModFile.Print($"Was this icon tracked for destruction? {destroyedIcons.Contains(__instance)}");
            if (__instance.target == null)
            {
                MainModFile.Print($"Target was null!");
                UnityEngine.Debug.Log(new System.Diagnostics.StackTrace());
                __instance.SetValue(default);
                return false;
            }
            if (__instance.target.statusEffects == null)
            {
                MainModFile.Print($"Target status effects was null!");
                UnityEngine.Debug.Log(new System.Diagnostics.StackTrace());
                __instance.SetValue(default);
                return false;
            }
            if (__instance.type == null)
            {
                MainModFile.Print($"Target type was null!");
                UnityEngine.Debug.Log(new System.Diagnostics.StackTrace());
                __instance.SetValue(default);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(EntityDisplay), "SetStatusIcon")]
    internal static class InsertTestPatch
    {
        static void Test(EntityDisplay __instance, string type, StatusIcon icon)
        {
            MainModFile.Print($"We got stuff: {__instance}, {type}, {icon}");
            MainModFile.Print($"Icon value was: {icon.value.current} before SetValue was called");
        }

        static HarmonyReturn Test2(EntityDisplay __instance, string iconGroupName, ref StatusIcon icon)
        {
            MainModFile.Print($"We got stuff: {__instance}, {icon}");
            string type = icon.type;
            MainModFile.Print($"Icon Type: {icon.GetType()}, type: {icon.type}");
            if (icon.GetType() != typeof(StatusIcon))
            {
                MainModFile.Print($"Different type, just return to prevent issues");
                return HarmonyReturn.Continue();
            }
            if (icon.type == "health" || icon.type == "damage")
            {
                MainModFile.Print($"Dont touch health or damage");
                return HarmonyReturn.Continue();
            }
            if (icon.type == "scrap")
            {
                MainModFile.Print($"Its already scrap");
                return HarmonyReturn.Continue();
            }

            MainModFile.Print($"Mark original Icon for destruction");
            //DebugTest.destroyedIcons.Add(icon);
            icon.Assign(MainModFile.instance.dummyEntity);
            icon.SetValue(default);
            icon.Destroy();
            if (type == "shroom")
            {
                MainModFile.Print($"Return nothing for shroom icon");
                return HarmonyReturn.Return(null);
            }
            MainModFile.Print($"Make new icon with scrap image");
            icon = CardManager.NewStatusIcon("scrap", __instance.iconGroups[iconGroupName]);
            MainModFile.Print($"Set icon type back to {type}");
            icon.type = type;
            MainModFile.Print($"Temporarily set icon value to 99, it gets reset later by SetValue");
            icon.value = new Stat(99, 99);
            return HarmonyReturn.Continue();
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original, ILGenerator generator)
        {
            return new HarmonyInsertPatch<EntityDisplay>(instructions, original, generator)
                .WithParams("type")
                .WithLocals("icon")
                .FindFirstMatch(new Matcher.MethodCallMatcher(AccessTools.Method(typeof(StatusIcon), "Assign")))
                .ApplyPatch(AccessTools.Method(typeof(InsertTestPatch), "Test"))
                .WithParams("iconGroupName")
                .WithLocals("icon")
                .FindFirstMatch(new Matcher.FieldAccessMatcher(AccessTools.Field(typeof(EntityDisplay), "hover")))
                .ApplyPatch(AccessTools.Method(typeof(InsertTestPatch), "Test2"))
                .Compile();
        }
    }
}
