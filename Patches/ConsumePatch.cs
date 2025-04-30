using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Spirefrost.Patches
{
    [HarmonyPatch(typeof(StatusEffectDestroyAfterUse), "CheckAction")]
    internal class ConsumePatch
    {
        static void Redirect(StatusEffectDestroyAfterUse __instance)
        {
            if (__instance.target.statusEffects.Where(status => status is StatusEffectRedirectConsume).Count() == 0 && __instance.destroy)
            {
                Entity redirect = References.Player.handContainer.Where(e => e.statusEffects.Where(status => status is StatusEffectRedirectConsume).Count() > 0).FirstOrDefault();
                if (redirect != null)
                {
                    __instance.destroy = false;
                    redirect.alive = false;
                    ActionQueue.Stack(new ActionConsume(redirect));
                }
            }
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            FieldInfo destroy = AccessTools.Field(typeof(StatusEffectDestroyAfterUse), "destroy");

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldfld)
                {
                    if (codes[i].operand is FieldInfo info && info == destroy)
                    {
                        Debug.Log("ConsumePatch - Match found, injecting new instructions");
                        // Ldarg0 already put "this" on the stack
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ConsumePatch), "Redirect"));
                        // Put "this" back on the stack
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                    }
                }
                yield return codes[i];
            }
        }
    }
}
