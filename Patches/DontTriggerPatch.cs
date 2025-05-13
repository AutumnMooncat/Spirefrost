using HarmonyLib;

namespace Spirefrost.Patches
{
    // IEnumerator, but we only want Pre so it should be fine
    [HarmonyPatch(typeof(ActionProcessTrigger), "Run")]
    internal class DontTriggerPatch
    {
        static bool Prefix(ActionProcessTrigger __instance)
        {
            bool ignore = false;
            Trigger trigger = __instance.trigger;
            if (trigger == null && __instance.GetTriggerMethod != null)
            {
                trigger = __instance.GetTriggerMethod();
            }
            SpirefrostEvents.InvokeIgnoreTriggerCheck(ref trigger, ref ignore);
            __instance.trigger = trigger;
            return !ignore;
        }
    }
}
