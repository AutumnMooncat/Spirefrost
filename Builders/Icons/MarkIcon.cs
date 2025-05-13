using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using TMPro;
using UnityEngine;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.Icons
{
    internal class MarkIcon : SpirefrostBuilder
    {
        internal static string ID => "STS Mark Icon";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static string SpriteID => "spirefrost.stsmark";

        internal static string DamageID => "damage." + SpriteID;

        internal static object GetBuilder()
        {
            return new StatusIconBuilder(MainModFile.instance)
                .Create(ID, SpriteID, MainModFile.instance.ImagePath("Icons/MarkIcon.png"))
                .WithIconGroupName(StatusIconBuilder.IconGroups.health)
                .WithTextColour(new Color(0.2471f, 0.1216f, 0.1647f, 1f))
                .WithTextShadow(new Color(1.0f, 1.0f, 1.0f, 1.0f))
                .WithTextboxSprite()
                .WithKeywords(MarkKeyword.ID)
                .WithEffectDamageSFX(MainModFile.instance.ImagePath("SFX/Fire.ogg"))
                .FreeModify(icon =>
                {
                    icon.textElement.outlineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    icon.textElement.outlineWidth = 0.2f;
                    icon.textElement.fontSharedMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.25f);

                    GameObject vfx = new SpirefrostVFXBuilder(MainModFile.instance, "Icons/MarkIcon.png")
                    .WithColorGradient(Color.white, Color.white, new Color(1, 1, 1, 0))
                    .WithSizeGradient(true, 2f, 3f, 0f)
                    .WithDuration(1f)
                    .Build();
                    vfx.RegisterAsDamageEffect(DamageID);
                });
        }
    }
}
