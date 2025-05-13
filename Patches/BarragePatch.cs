using HarmonyLib;
using System.Collections.Generic;

namespace Spirefrost.Patches
{
    [HarmonyPatch(typeof(TargetModeRow), "AddEligible")]
    internal class BarragePatch
    {
        public static bool shortCircuit;

        public static bool Prefix(ISet<Entity> targets, IEnumerable<Entity> fromCollection)
        {
            if (shortCircuit)
            {
                foreach (Entity item in fromCollection)
                {
                    targets.Add(item);
                }
                return false;
            }
            return true;
        }
    }
}
