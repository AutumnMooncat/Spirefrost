using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spirefrost.Patches
{
    internal class AlreadyAttackingPatches
    {
        private static readonly Dictionary<Entity, bool> attackDict = new Dictionary<Entity, bool>();

        internal static bool HasAlreadyAttacked(Entity entity)
        {
            return attackDict.GetValueOrDefault(entity, false);
        }

        [HarmonyPatch(typeof(ActionQueue), "PostAction")]
        private class ClearAfterActions
        {
            static void Postfix()
            {
                if (ActionQueue.Empty)
                {
                    attackDict.Clear();
                }
            }
        }

        [HarmonyPatch(typeof(StatusEffectSystem), "CardPlayedEvent")]
        private class AddAfterTrigger
        {
            static void Postfix(Entity entity)
            {
                attackDict[entity] = true;
            }
        }
    }
}
