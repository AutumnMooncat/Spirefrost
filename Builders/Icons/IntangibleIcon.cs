using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using TMPro;
using UnityEngine;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.Icons
{
    internal class IntangibleIcon : SpirefrostBuilder
    {
        internal static string ID => "STS Intangible Icon";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static string SpriteID => "spirefrost.stsintangible";

        internal static object GetBuilder()
        {
            return new StatusIconBuilder(MainModFile.instance)
                .Create(ID, SpriteID, MainModFile.instance.ImagePath("Icons/IntangibleIcon.png"))
                .WithIconGroupName(StatusIconBuilder.IconGroups.health)
                //.WithTextColour(new Color(0.2471f, 0.1216f, 0.1647f, 1f))
                //.WithTextShadow(new Color(1.0f, 1.0f, 1.0f, 1.0f))
                .WithTextboxSprite()
                .WithKeywords(IntangibleKeyword.ID)
                .WithApplySFX(MainModFile.instance.ImagePath("SFX/Intangible.ogg"))
                .FreeModify(icon =>
                {
                    //icon.textElement.outlineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    //icon.textElement.outlineWidth = 0.2f;
                    //icon.textElement.fontSharedMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.25f);
                    icon.textElement = null;

                    GameObject vfx = new SpirefrostVFXBuilder(MainModFile.instance, "Icons/IntangibleIcon.png")
                    .WithColorGradient(Color.white, Color.white, new Color(1, 1, 1, 0))
                    .WithSizeGradient(true, 2f, 3f)
                    .WithDuration(1f)
                    .Build();
                    vfx.RegisterAsApplyEffect(icon.type);
                });
        }
    }
}
