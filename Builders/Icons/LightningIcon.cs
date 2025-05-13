using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Patches;
using TMPro;
using UnityEngine;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.Icons
{
    internal class LightningIcon : SpirefrostBuilder
    {
        internal static string ID => "STS Lightning Icon";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static string SpriteID => "spirefrost.stslightning";

        internal static string DamageID => "damage." + SpriteID;

        internal static object GetBuilder()
        {
            return new StatusIconBuilder(MainModFile.instance)
                .CreateCustom(ID, SpriteID, MainModFile.instance.ImagePath("Icons/LightningIcon.png"))
                .WithIconGroupName(LayoutPatch.orbIconGroup)
                .WithTextColour(new Color(0.2471f, 0.1216f, 0.1647f, 1f))
                .WithTextShadow(new Color(1.0f, 1.0f, 1.0f, 1.0f))
                .WithTextboxSprite()
                .WithKeywords(LightningKeyword.ID)
                .WithApplySFX(MainModFile.instance.ImagePath("SFX/LightningChannel.ogg"))
                .WithEffectDamageSFX(MainModFile.instance.ImagePath("SFX/LightningPassive.ogg"))
                .FreeModify(icon =>
                {
                    icon.textElement.outlineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    icon.textElement.outlineWidth = 0.2f;
                    icon.textElement.fontSharedMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.25f);

                    GameObject vfxApply = new SpirefrostVFXBuilder(MainModFile.instance, "Icons/LightningIcon.png")
                    .WithColorGradient(Color.white, Color.white, new Color(1, 1, 1, 0))
                    .WithSizeGradient(true, 2f, 3f, 0f)
                    .WithDuration(1f)
                    .WithVelocityGradient(Vector3.zero, Vector3.zero, new Vector3(0, 7.5f, 0))
                    .Build();
                    vfxApply.RegisterAsApplyEffect(icon.type);

                    GameObject vfxDamage = new SpirefrostVFXBuilder(MainModFile.instance, "VFX/lightning.png")
                    .WithColorGradient(Color.white, Color.yellow, new Color(1, 1, 1, 0))
                    .WithSizeGradient(true, 1f, 1.2f, 1f)
                    .WithDuration(1f)
                    .WithInitialOffset(new Vector3(0, 15, 0))
                    .WithVelocityGradient(new Vector3(0, -100, 0), Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero)
                    .Build();
                    vfxDamage.RegisterAsDamageEffect(DamageID);
                });
        }
    }
}
