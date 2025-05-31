using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Patches;
using TMPro;
using UnityEngine;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.Icons
{
    internal class DarkIcon : SpirefrostBuilder
    {
        internal static string ID => "STS Dark Icon";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static string SpriteID => "spirefrost.stsdark";

        internal static string DamageID => "damage." + SpriteID;

        internal static object GetBuilder()
        {
            return new StatusIconBuilder(MainModFile.instance)
                .Create(ID, SpriteID, MainModFile.instance.ImagePath("Icons/DarkIcon.png"))
                .WithIconGroupName(LayoutPatch.orbIconGroup)
                .WithTextColour(new Color(0.2471f, 0.1216f, 0.1647f, 1f))
                .WithTextShadow(new Color(1.0f, 1.0f, 1.0f, 1.0f))
                .WithTextboxSprite()
                .WithKeywords(DarkKeyword.ID)
                .WithApplySFX(MainModFile.instance.ImagePath("SFX/DarkChannel.ogg"))
                .WithEffectDamageSFX(MainModFile.instance.ImagePath("SFX/DarkEvoke.ogg"))
                .FreeModify(icon =>
                {
                    icon.textElement.outlineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    icon.textElement.outlineWidth = 0.2f;
                    icon.textElement.fontSharedMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.25f);

                    GameObject vfxApply = new SpirefrostVFXBuilder(MainModFile.instance, "Icons/DarkIcon.png")
                    .WithColorGradient(Color.white, Color.white, new Color(1, 1, 1, 0))
                    .WithSizeGradient(true, 2f, 3f, 0f)
                    .WithDuration(1f)
                    .WithVelocityGradient(Vector3.zero, Vector3.zero, new Vector3(0, 7.5f, 0))
                    .Build();
                    vfxApply.RegisterAsApplyEffect(icon.type);

                    GameObject vfxDamage = new SpirefrostVFXBuilder(MainModFile.instance, "VFX/orbFlareOuter.png")
                    .WithColorGradient(new Color(0.5f, 0.4f, 0.8f, 1), new Color(0.5f, 0.4f, 0.8f, 1), new Color(0.5f, 0.4f, 0.8f, 0))
                    .WithSizeGradient(true, new Vector3(4, 3, 4), new Vector3(4, 6, 4), new Vector3(4, 3, 4))
                    .WithDuration(1f)
                    .WithEffects(new SpirefrostVFXBuilder(MainModFile.instance, "VFX/orbFlareInner.png")
                        .WithColorGradient(new Color(0.4f, 0.32f, 0.64f, 1), new Color(0.4f, 0.32f, 0.64f, 1), new Color(0.4f, 0.32f, 0.64f, 0))
                        .WithSizeGradient(true, new Vector3(3, 2, 3), new Vector3(3, 4, 3), new Vector3(3, 2, 3))
                        .WithDuration(1f)
                        .Build())
                    .Build();
                    vfxDamage.RegisterAsDamageEffect(DamageID);
                });
        }
    }
}
