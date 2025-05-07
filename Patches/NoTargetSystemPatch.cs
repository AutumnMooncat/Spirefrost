using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Patches
{
    [HarmonyPatch]
    internal class NoTargetSystemPatch
    {
        internal enum NoTargetTypeExtra
        {
            None,
            NoItemsToMove,
            NoCompanionsToMove,
            NoClunkersToMove,
            NoCardsToMove
        }

        internal static NoTargetTypeExtra noTargetType;

        static string GetText(String original, params object[] args)
        {
            switch(noTargetType)
            {
                case NoTargetTypeExtra.NoItemsToMove:
                    original = LocalizationHelper.GetCollection("UI Text", SystemLanguage.English)
                        .GetString(SpirefrostStrings.NoItemsToMove)
                        .GetLocalizedString();
                    break;
                case NoTargetTypeExtra.NoCompanionsToMove:
                    original = LocalizationHelper.GetCollection("UI Text", SystemLanguage.English)
                        .GetString(SpirefrostStrings.NoCompanionsToMove)
                        .GetLocalizedString();
                    break;
                case NoTargetTypeExtra.NoClunkersToMove:
                    original = LocalizationHelper.GetCollection("UI Text", SystemLanguage.English)
                        .GetString(SpirefrostStrings.NoClunkersToMove)
                        .GetLocalizedString();
                    break;
                case NoTargetTypeExtra.NoCardsToMove:
                    original = LocalizationHelper.GetCollection("UI Text", SystemLanguage.English)
                        .GetString(SpirefrostStrings.NoCardsToMove)
                        .GetLocalizedString();
                    break;
            }
            noTargetType = NoTargetTypeExtra.None;
            return original.Format(args);
        }

        private static Type createdType;
        static MethodBase TargetMethod()
        {
            return SpirefrostUtils.FindEnumeratorMethod(AccessTools.DeclaredMethod(typeof(NoTargetTextSystem), "_Run"), ref createdType);
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            FieldInfo args = AccessTools.Field(createdType, "args");
            MethodInfo setText = AccessTools.Method(typeof(TMPro.TMP_Text), "set_text");
            MethodInfo getText = AccessTools.Method(typeof(NoTargetSystemPatch), "GetText");

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt)
                {
                    if (codes[i].operand is MethodInfo info && info == setText)
                    {
                        Debug.Log("NoTargetSystemPatch - Match found, injecting new instructions");
                        // String already on the stack
                        CodeInstruction nop = new CodeInstruction(OpCodes.Nop);
                        nop.labels.AddRange(codes[i].labels);
                        codes[i].labels.Clear();
                        yield return nop;
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, args);
                        yield return new CodeInstruction(OpCodes.Call, getText);
                        // String back on the stack
                    }
                }
                yield return codes[i];
            }
        }
    }
}
