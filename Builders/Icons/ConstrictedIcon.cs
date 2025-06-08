using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using TMPro;
using UnityEngine;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.Icons
{
    internal class ConstrictedIcon : SpirefrostBuilder
    {
        internal static string ID => "STS Constricted Icon";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static string SpriteID => "spirefrost.stsconstricted";

        internal static string DamageID => "damage." + SpriteID;

        internal static object GetBuilder()
        {
            return new StatusIconBuilder(MainModFile.instance)
                .Create(ID, SpriteID, MainModFile.instance.ImagePath("Icons/ConstrictedIcon.png"))
                .WithIconGroupName(StatusIconBuilder.IconGroups.health)
                .WithTextColour(new Color(0.2471f, 0.1216f, 0.1647f, 1f))
                .WithTextShadow(new Color(1.0f, 1.0f, 1.0f, 1.0f))
                .WithTextboxSprite()
                .WithKeywords(ConstrictedKeyword.ID)
                .WithApplySFX(MainModFile.instance.ImagePath("SFX/Buff.ogg"))
                .WithEffectDamageSFX(MainModFile.instance.ImagePath("SFX/Constrict.ogg"))
                .FreeModify(icon =>
                {
                    icon.textElement.outlineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    icon.textElement.outlineWidth = 0.2f;
                    icon.textElement.fontSharedMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.25f);

                    GameObject applyVFX = new SpirefrostVFXBuilder(MainModFile.instance, "Icons/ConstrictedIcon.png")
                    .WithColorGradient(Color.white, Color.white, new Color(1, 1, 1, 0))
                    .WithSizeGradient(true, 2f, 3f, 0f)
                    .WithDuration(1f)
                    .Build();
                    applyVFX.RegisterAsApplyEffect(icon.type);

                    GameObject damageVFX = new SpirefrostVFXBuilder(MainModFile.instance, "Icons/ConstrictedIcon.png")
                    .WithColorGradient(Color.white, Color.white, new Color(1, 1, 1, 0))
                    .WithSizeGradient(true, new Vector2(2.5f, 2.5f), new Vector2(1.75f, 3.25f), new Vector2(1.5f, 3.5f), new Vector2(1.75f, 3.25f), new Vector2(2.5f, 2.5f))
                    .WithDuration(1f)
                    .Build();
                    damageVFX.RegisterAsDamageEffect(DamageID);
                });
        }
    }
}
