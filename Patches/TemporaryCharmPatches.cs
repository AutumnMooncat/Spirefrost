using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Spirefrost.Patches
{
    [HarmonyPatch]
    internal class TemporaryCharmPatches
    {
        [HarmonyPatch(typeof(InjurySystem), nameof(InjurySystem.EntityKilled))]
        internal class EntityKilledPatch
        {
            static CardData GetActualCardData(CardData got, Entity entity)
            {
                return entity.GetOriginalData() ?? got;
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> codes = instructions.ToList();
                MethodInfo getActual = AccessTools.Method(typeof(EntityKilledPatch), nameof(GetActualCardData));
                MethodInfo dataGetter = AccessTools.PropertyGetter(typeof(Entity), nameof(Entity.data));
                for (int i = 0; i < codes.Count; i++)
                {
                    yield return codes[i];
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand as MethodInfo == dataGetter)
                    {
                        Debug.Log($"EntityKilledPatch - match found, wrapping data in check");
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Call, getActual);
                    }
                }
            }
        }
    }
}
