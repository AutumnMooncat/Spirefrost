using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Spirefrost.Patches
{
    [HarmonyPatch]
    internal class EntityEnabledPatches
    {
        static bool wasDisabled;
        static Entity check;

        [HarmonyPatch(typeof(Sequences), nameof(Sequences.CardMove))]
        internal class CardMovePatch
        {
            internal static IEnumerator Postfix(IEnumerator __result, Entity entity)
            {
                yield return __result;
                if (wasDisabled && entity == check && entity.IsAliveAndExists())
                {
                    entity.enabled = true;
                }
                check = null;
                wasDisabled = false;
            }
        }

        [HarmonyPatch(typeof(CardPocket), nameof(CardPocket.CardAdded))]
        internal class CardPocketPatch
        {
            internal static void Postfix(Entity entity)
            {
                wasDisabled = true;
                check = entity;
            }
        }

        [HarmonyPatch(typeof(ActionRevealAll), nameof(ActionRevealAll.Process))]
        internal class RevealTriggersEnablePatch
        {
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                MethodInfo insert = AccessTools.Method(typeof(ActionQueue), nameof(ActionQueue.Insert));
                MethodInfo getEnabled = AccessTools.PropertyGetter(typeof(Behaviour), nameof(Behaviour.enabled));
                MethodInfo add = AccessTools.Method(typeof(HashSet<Entity>), nameof(HashSet<Entity>.Add));
                bool inserted = false;
                object found = null;
                for (int i = 0; i < codes.Count; i++)
                {
                    yield return codes[i];
                    if (!inserted && codes[i].operand as MethodInfo == insert)
                    {
                        inserted = true;
                        int curr = i - 1;
                        while (found == null && curr >= 0)
                        {
                            if (codes[curr].IsBrfalse())
                            {
                                found = codes[curr].operand;
                            }
                            curr--;
                        }
                        if (found == null)
                        {
                            Debug.Log($"RevealTriggersEnablePatch - failed to find label");
                            continue;
                            
                        }
                        Debug.Log($"RevealTriggersEnablePatch - match found, inserting instructions");
                        // junk to pop already on the stack
                        yield return new CodeInstruction(OpCodes.Pop);
                        yield return new CodeInstruction(OpCodes.Ldloc, 7); // item
                        yield return new CodeInstruction(OpCodes.Callvirt, getEnabled);
                        yield return new CodeInstruction(OpCodes.Brfalse, found);
                        yield return new CodeInstruction(OpCodes.Ldloc_0);
                        yield return new CodeInstruction(OpCodes.Ldloc, 7); // item
                        yield return new CodeInstruction(OpCodes.Callvirt, add);
                        // Leave junk to pop on the stack
                    }
                }
            }
        }
    }
}
