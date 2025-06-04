using Spirefrost.Patches;
using System.Collections.Generic;
using UnityEngine;

namespace Spirefrost.StatusEffects
{
    public class StatusEffectSTSFocus : StatusEffectData
    {
        private readonly Dictionary<string, int> modifedTempEffects = new Dictionary<string, int>();

        public override void Init()
        {
            SpirefrostEvents.OnPreStatusReduction += ModifyReduction;
        }

        public void OnDestroy()
        {
            SpirefrostEvents.OnPreStatusReduction -= ModifyReduction;
        }

        private void ModifyReduction(StatusEffectData status, ref int amount, bool temporary)
        {
            if (temporary && ShouldApply(status) && status.target == target)
            {
                if (modifedTempEffects.ContainsKey(status.name))
                {
                    amount += modifedTempEffects[status.name];
                    modifedTempEffects.Remove(status.name);
                }
            }
        }

        public override bool RunApplyStatusEvent(StatusEffectApply apply)
        {
            if (ShouldApply(apply?.effectData) && apply.target == target)
            {
                apply.count += GetAmount();
                if (StatusSystemPatch.isTemp)
                {
                    modifedTempEffects[apply.effectData.name] = GetAmount() + modifedTempEffects.GetValueOrDefault(apply.effectData.name, 0);
                }
            }

            return false;
        }

        private bool ShouldApply(StatusEffectData apply)
        {
            return apply != null && apply.isStatus && apply.visible && !apply.offensive;
        }
    }
}
