using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using TMPro;
using UnityEngine;
using WildfrostHopeMod.SFX;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.Icons
{
    internal class RitualIcon : SpirefrostBuilder
    {
        internal static string ID => "STS Ritual Icon";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static string SpriteID => "spirefrost.stsritual";

        internal static string CawCawID => "trigger." + SpriteID;

        internal static object GetBuilder()
        {
            return new StatusIconBuilder(MainModFile.instance)
                .Create(ID, SpriteID, MainModFile.instance.ImagePath("Icons/RitualIcon.png"))
                .WithIconGroupName(StatusIconBuilder.IconGroups.damage)
                .WithTextColour(new Color(0.2471f, 0.1216f, 0.1647f, 1f))
                .WithTextShadow(new Color(1.0f, 1.0f, 1.0f, 1.0f))
                .WithTextboxSprite()
                .WithKeywords(RitualKeyword.ID)
                .WithApplySFX(MainModFile.instance.ImagePath("SFX/Buff.ogg"))
                .FreeModify(icon =>
                {
                    icon.textElement.outlineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    icon.textElement.outlineWidth = 0.2f;
                    icon.textElement.fontSharedMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.25f);

                    GameObject vfx = new SpirefrostVFXBuilder(MainModFile.instance, "Icons/RitualIcon.png")
                    .WithColorGradient(Color.white, Color.white, new Color(1, 1, 1, 0))
                    .WithSizeGradient(true, 2f, 3f)
                    .WithDuration(1f)
                    .Build();
                    vfx.RegisterAsApplyEffect(icon.type);

                    SFXLoader loader = VFXMod.instance?.SFX;
                    if (loader != null)
                    {
                        SFXLoader.RegisterSoundToGlobal(CawCawID, loader.LoadSoundFromPath(MainModFile.instance.ImagePath("SFX/CawCaw.ogg")), 0.05f);
                    }
                });
        }
    }
}
