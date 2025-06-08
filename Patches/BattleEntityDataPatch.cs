using HarmonyLib;
using Spirefrost.StatusEffects;
using System.Linq;

namespace Spirefrost.Patches
{
    [HarmonyPatch(typeof(BattleEntityData), MethodType.Constructor, typeof(Entity))]
    internal class BattleEntityDataPatch
    {
        static void Postfix(BattleEntityData __instance, Entity entity)
        {
            foreach (var item in entity.statusEffects)
            {
                if (item is StatusEffectOngoingAttackEffect ongoing)
                {
                    StatusEffectSaveData attackEffect = __instance.attackEffects.FirstOrDefault(e => e.name == ongoing.effect.name);
                    attackEffect.count -= ongoing.ManuallyAdded;
                    if (attackEffect.count <= 0)
                    {
                        __instance.attackEffects = __instance.attackEffects.Without(attackEffect);
                    }
                }
            }
        }
    }
}
