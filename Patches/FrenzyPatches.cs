using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Spirefrost.Patches
{
    [HarmonyPatch(typeof(StatusEffectMultiHit), "RunCardPlayedEvent")]
    internal class FrenzyCardPlayedPatch
    {
        internal static bool CountdownCheck(StatusEffectMultiHit __instance)
        {
            //Debug.Log($"FrenzyCardPlayedPatch - Checking current action {ActionQueue.current}");
            foreach (var item in ActionTriggerPatch.actionMap)
            {
                if (ActionQueue.current == item.Value)
                {
                    //Debug.Log($"FrenzyCardPlayedPatch - Action found in action map");
                    if (__instance.additionalTriggers != null && __instance.additionalTriggers.Contains(item.Key))
                    {
                        //Debug.Log($"FrenzyCardPlayedPatch - Action found in {__instance}, count down");
                        return true;
                    }
                }
            }

            //Debug.Log($"FrenzyCardPlayedPatch - Action not found in {__instance}");
            return false;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            Label jump = generator.DefineLabel();
            CodeInstruction nopJump = new CodeInstruction(OpCodes.Nop);
            nopJump.labels.Add(jump);
            MethodInfo countdownCheck = AccessTools.Method(typeof(FrenzyCardPlayedPatch), "CountdownCheck");
            MethodInfo isSnowed = AccessTools.Method(typeof(Entity), "get_IsSnowed");
            FieldInfo attackCount = AccessTools.Field(typeof(StatusEffectMultiHit), "attackCount");
            bool checkInserted = false;
            bool jumpInserted = false;
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldarg_0 && i + 2 < codes.Count && codes[i + 1].opcode == OpCodes.Ldarg_0 && codes[i + 2].opcode == OpCodes.Ldfld && codes[i + 2].operand is FieldInfo finfo && finfo == attackCount)
                {
                    if (!checkInserted)
                    {
                        Debug.Log("FrenzyCardPlayedPatch - Match found, inserting codes");
                        checkInserted = true;
                        // Stack empty
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, countdownCheck);
                        yield return new CodeInstruction(OpCodes.Brfalse, jump);
                    }
                }

                if (codes[i].opcode == OpCodes.Ldarg_1 && i + 1 < codes.Count && codes[i + 1].opcode == OpCodes.Callvirt && codes[i + 1].operand is MethodInfo minfo && minfo == isSnowed)
                {
                    if (!jumpInserted)
                    {
                        Debug.Log("FrenzyCardPlayedPatch - Match found, inserting jump");
                        jumpInserted = true;
                        yield return nopJump;
                    }
                }

                yield return codes[i];
            }
        }
    }

    [HarmonyPatch(typeof(StatusEffectMultiHit), "EntityTrigger")]
    internal class FrenzyEntityTriggerPatch
    {
        internal static readonly Dictionary<StatusEffectMultiHit, List<ActionTrigger>> triggerMap = new Dictionary<StatusEffectMultiHit, List<ActionTrigger>>();

        internal static bool TriggerCheck(StatusEffectMultiHit __instance, Trigger original, Trigger param)
        {
            if (IsTriggerFromMultiHit(param))
            {
                return true;
            }
            return __instance.target.IsSnowed;
        }

        internal static bool IsTriggerFromMultiHit(Trigger trigger)
        {
            foreach (var pair in triggerMap)
            {
                if (pair.Value.Any(action => ActionTriggerPatch.actionMap.ContainsKey(action) && ActionTriggerPatch.actionMap[action].trigger == trigger))
                {
                    return true;
                }
            }
            return false;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            Label jump = generator.DefineLabel();
            CodeInstruction nopJump = new CodeInstruction(OpCodes.Nop);
            nopJump.labels.Add(jump);
            MethodInfo triggerCheck = AccessTools.Method(typeof(FrenzyEntityTriggerPatch), "TriggerCheck");
            FieldInfo origTrigger = AccessTools.Field(typeof(StatusEffectMultiHit), "originalTrigger");
            FieldInfo attackCount = AccessTools.Field(typeof(StatusEffectMultiHit), "attackCount");
            bool loadInserted = false;
            bool postLoadInserted = false;
            bool checkInserted = false;
            bool jumpInserted = false;
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_1 && i + 1 < codes.Count && codes[i + 1].opcode == OpCodes.Stfld && codes[i + 1].operand is FieldInfo finfo1 && finfo1 == attackCount)
                {
                    if (!checkInserted)
                    {
                        checkInserted = true;
                        Debug.Log("FrenzyEntityTriggerPatch - Match found, inserting check codes and noping current");
                        // Stack has "this", which we will use
                        yield return new CodeInstruction(OpCodes.Ldfld, origTrigger);
                        yield return new CodeInstruction(OpCodes.Brtrue, jump);
                        codes[i] = new CodeInstruction(OpCodes.Nop);
                        codes[i + 1] = new CodeInstruction(OpCodes.Nop);
                    }
                }

                if (codes[i].opcode == OpCodes.Ldarg_1 && i > 0 && codes[i - 1].opcode == OpCodes.Stfld && codes[i - 1].operand is FieldInfo finfo2 && finfo2 == origTrigger)
                {
                    if (!jumpInserted)
                    {
                        jumpInserted = true;
                        Debug.Log("FrenzyEntityTriggerPatch - Match found, inserting jump codes");
                        yield return nopJump;
                    }
                }

                if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand is FieldInfo finfo3 && finfo3 == origTrigger && i + 1 < codes.Count && (codes[i + 1].opcode == OpCodes.Brfalse_S || codes[i + 1].opcode == OpCodes.Brfalse))
                {
                    if (!loadInserted)
                    {
                        Debug.Log("FrenzyEntityTriggerPatch - Match found, inserting load codes");
                        loadInserted = true;
                        // Stack has "this" but we need an additional "this" for the method call
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                    }
                }

                yield return codes[i];

                if (loadInserted && !postLoadInserted)
                {
                    postLoadInserted = true;
                    // Stack has "this", originalTrigger
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldind_Ref);
                    yield return new CodeInstruction(OpCodes.Call, triggerCheck);
                }
            }
        }
    }

    [HarmonyPatch(typeof(StatusEffectMultiHit), "AddTrigger")]
    internal class FrenzyAddTriggerPatch
    {
        static void Prefix(StatusEffectMultiHit __instance, ActionTrigger trigger)
        {
            if (!FrenzyEntityTriggerPatch.triggerMap.ContainsKey(__instance))
            {
                FrenzyEntityTriggerPatch.triggerMap[__instance] = new List<ActionTrigger>();
            }

            FrenzyEntityTriggerPatch.triggerMap[__instance].Add(trigger);
        }
    }

    [HarmonyPatch]
    internal class ActionTriggerPatch
    {
        internal static readonly Dictionary<ActionTrigger, ActionProcessTrigger> actionMap = new Dictionary<ActionTrigger, ActionProcessTrigger>();

        internal static ActionProcessTrigger MapTrigger(ActionProcessTrigger created, ActionTrigger __instance)
        {
            foreach (var item in FrenzyEntityTriggerPatch.triggerMap)
            {
                if (item.Value.Contains(__instance))
                {
                    actionMap[__instance] = created;
                }
            }
            
            return created;
        }

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(ActionTrigger), nameof(ActionTrigger.Process));
            yield return AccessTools.Method(typeof(ActionTriggerAgainst), nameof(ActionTriggerAgainst.Process));
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            MethodInfo mapTrigger = AccessTools.Method(typeof(ActionTriggerPatch), "MapTrigger");
            MethodInfo stack = AccessTools.Method(typeof(ActionQueue), "Stack");
            bool loadInserted = false;
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_0 && i + 1 < codes.Count && codes[i + 1].opcode == OpCodes.Call && codes[i + 1].operand is MethodInfo info && info == stack)
                {
                    if (!loadInserted)
                    {
                        Debug.Log("ActionTriggerPatch - Match found, inserting codes");
                        loadInserted = true;
                        // Stack has ActionProcessTrigger, but we also need "this"
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, mapTrigger);
                    }
                }

                yield return codes[i];
            }
        }
    }

    [HarmonyPatch(typeof(StatusEffectMultiHit), "Cancel")]
    internal class FrenzyCancelPatch
    {
        static void Prefix(StatusEffectMultiHit __instance)
        {
            if (FrenzyEntityTriggerPatch.triggerMap.ContainsKey(__instance))
            {
                foreach (var item in FrenzyEntityTriggerPatch.triggerMap[__instance])
                {
                    ActionTriggerPatch.actionMap.Remove(item);
                }

                FrenzyEntityTriggerPatch.triggerMap.Remove(__instance);
            }
        }
    }
}
