using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Core.Settings;

namespace Spirefrost.Patches
{
    [HarmonyPatch(typeof(Text), nameof(Text.Process), new Type[] { typeof(string), typeof(int), typeof(float), typeof(Text.ColourProfileHex) })]
    internal class DynamicTextPatches
    {
        private static readonly string regex = "\\{!.*?!\\|.*?}";
        private static void HandleDynText(ref string text, int effectBonus, float effectFactor)
        {
            //Debug.Log($"Got: {text}");
            var matches = Regex.Matches(text, regex);
            for (int i = 0; i < matches.Count; i++) 
            {
                //Debug.Log($"Found match: {matches[i].Value}");
                Match match = matches[i];
                text = text.Replace(match.Value, UnwrapText(match.Value, effectBonus, effectFactor));
            }
        }

        private static string UnwrapText(string found, int effectBonus, float effectFactor)
        {
            string ret = "";
            // Trim { and }
            //Debug.Log($"Processing {found}");
            found = found.Substring(1, found.Length - 2);
            //Debug.Log($"Trimmed to {found}");
            // Split into cases
            string[] chunks = found.Split('|');
            //Debug.Log($"Split into {chunks.Length} chunks");
            //Debug.Log($"Attempting to parse {chunks[0].Substring(1, chunks[0].Length - 2)}");
            // First match will be our amount wrapped by !, trim them and parse
            if (int.TryParse(chunks[0].Substring(1, chunks[0].Length - 2), out int result))
            {
                // Get effective amount
                //Debug.Log($"Parsed {result}");
                int num = Mathf.Max(0, Mathf.RoundToInt((result + effectBonus) * effectFactor));
                //Debug.Log($"Adjusted value is {num}");
                bool matched = false;
                // Check every case
                for (int i = 1; i < chunks.Length; i++)
                {
                    string chunk = chunks[i];
                    //Debug.Log($"Checking case {chunk}");
                    // Every case will contain an = to split value(s) -> output
                    if (chunk.Contains("="))
                    {
                        string[] split = chunk.Split(new char[] { '=' }, 2);
                        //Debug.Log($"Split into {split[0]} and {split[1]}");
                        // Handle all values if there are more than 1
                        string[] values = split[0].Split(',');
                        //Debug.Log($"Found {values.Length} values");
                        foreach (var item in values)
                        {
                            //Debug.Log($"Checking {item}");
                            if (CheckMatch(num, item))
                            {
                                //Debug.Log($"Matched! Setting output to {split[1]}");
                                matched = true;
                                ret = split[1];
                            }
                        }
                        // Set ret to default case if we havent matched yet
                        if (split[0].Equals("@") && !matched)
                        {
                            //Debug.Log($"Found default {split[1]}");
                            ret = split[1];
                        }
                    }
                }
            }
            //Debug.Log($"Returning {ret}");
            return ret;
        }

        private static bool CheckMatch(int num, string s)
        {
            // Greater Than, Less Than, Divisible By, and Ends With are the 4 supported conditional checks, in addition to Direct Match, which has no symbol attached
            bool greater = s.Contains(">");
            bool less = s.Contains("<");
            bool mod = s.Contains("%");
            bool ends = s.Contains("&");
            // Remove all these special symbols before checking if the condition is a number we can make
            s = s.Replace(">", "").Replace("<", "").Replace("%", "").Replace("&", "");
            if (int.TryParse(s, out int val))
            {
                // Grab our var minus the number, helpful for comparisons
                int comp = num - val;
                // Checks the Direct Match, Greater Than, and Less Than cases
                bool signCheck = !greater && !less && comp == 0 || greater && comp > 0 || less && comp < 0;
                // Checks the Divisible By case. If the numbers are equal (comp is 0), or if variable mod N is 0, then its divisible
                bool moduloCheck = mod && (comp == 0 || num % val == 0);
                // Checks the Ends With case. If the numbers are equal, or if the trailing digits of comp are all 0's, then var ends with N
                bool digitCheck = ends && (comp == 0 || comp % (int)Math.Pow(10, s.Length) == 0);
                // As long as at least one condition matches we are good
                if (signCheck || moduloCheck || digitCheck)
                {
                    return true;
                }
            }
            return false;
        }

        [HarmonyPatch(typeof(Text), nameof(Text.Process), new Type[] { typeof(string), typeof(int), typeof(float), typeof(Text.ColourProfileHex) })]
        static class ProcessPatch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> codes = instructions.ToList();
                MethodInfo handle = AccessTools.Method(typeof(DynamicTextPatches), nameof(HandleDynText));
                bool callInserted = false;
                for (int i = 0; i < codes.Count; i++)
                {
                    if (!callInserted && codes[i].opcode == OpCodes.Ldloc_0)
                    {
                        yield return new CodeInstruction(OpCodes.Ldloca, 0);
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Ldarg_2);
                        yield return new CodeInstruction(OpCodes.Call, handle);
                    }
                    yield return codes[i];
                }
            }
        }

        [HarmonyPatch]
        static class GetEffectTextPatch
        {
            static ErrorAction backup;

            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(Text), nameof(Text.GetEffectText), new Type[] { typeof(LocalizedString), typeof(string), typeof(int), typeof(bool) });
                yield return AccessTools.Method(typeof(Text), nameof(Text.GetEffectText), new Type[] { typeof(IEnumerable<LocalizedString>), typeof(string), typeof(int), typeof(bool) });
            }

            static void Prefix()
            {
                backup = LocalizationSettings.StringDatabase.SmartFormatter.Parser.Settings.ParseErrorAction;
                LocalizationSettings.StringDatabase.SmartFormatter.Parser.Settings.ParseErrorAction = ErrorAction.Ignore;
            }

            static void Postfix()
            {
                LocalizationSettings.StringDatabase.SmartFormatter.Parser.Settings.ParseErrorAction = backup;
            }
        }
    }
}
