using Spirefrost.Builders.Icons;
using UnityEngine;

namespace Spirefrost.StatusEffects
{
    public class StatusEffectEquipMask : StatusEffectData
    {
        public override void Init()
        {
            if (MainModFile.instance.maskedSpries.TryGetValue(target.data.name, out Sprite sprite))
            {
                if (target.display is Card card)
                {
                    card.mainImage.sprite = sprite;
                }
            }

            if (!BattleSaveSystem.instance.loading && MainModFile.instance.cawCaw)
            {
                SpirefrostUtils.PlayGlobalSound(RitualIcon.CawCawID);
            }
        }
    }
}
