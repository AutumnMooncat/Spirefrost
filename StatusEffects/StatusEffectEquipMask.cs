using UnityEngine;

namespace Spirefrost
{
    public class StatusEffectEquipMask : StatusEffectData
    {
        public override void Init()
        {
            if (MainModFile.instance.maskedSpries.TryGetValue(target.data.name, out Sprite sprite))
            {
                target.data.mainSprite = sprite;
                target.gameObject.GetComponent<Card>().mainImage.sprite = sprite;
            }
        }
    }
}
