using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Patches;
using TMPro;
using UnityEngine;
using WildfrostHopeMod.SFX;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.Icons
{
    internal class FrostIcon : SpirefrostBuilder
    {
        internal static string ID => "STS Frost Icon";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static string SpriteID => "spirefrost.stsfrost";

        internal static string EvokeID => "evoke." + SpriteID;

        internal static object GetBuilder()
        {
            return new StatusIconBuilder(MainModFile.instance)
                .Create(ID, SpriteID, MainModFile.instance.ImagePath("Icons/FrostIcon.png"))
                .WithIconGroupName(LayoutPatch.orbIconGroup)
                .WithTextColour(new Color(0.2471f, 0.1216f, 0.1647f, 1f))
                .WithTextShadow(new Color(1.0f, 1.0f, 1.0f, 1.0f))
                .WithTextboxSprite()
                .WithKeywords(FrostKeyword.ID)
                .WithApplySFX(MainModFile.instance.ImagePath("SFX/FrostChannel.ogg"))
                .FreeModify(icon =>
                {
                    icon.textElement.outlineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    icon.textElement.outlineWidth = 0.2f;
                    icon.textElement.fontSharedMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.25f);

                    GameObject vfx = new SpirefrostVFXBuilder(MainModFile.instance, "Icons/FrostIcon.png")
                    .WithColorGradient(Color.white, Color.white, new Color(1, 1, 1, 0))
                    .WithSizeGradient(true, 2f, 3f, 0f)
                    .WithDuration(1f)
                    .WithVelocityGradient(Vector3.zero, Vector3.zero, new Vector3(0, 7.5f, 0))
                    .Build();
                    vfx.RegisterAsApplyEffect(icon.type);

                    SFXLoader loader = VFXMod.instance?.SFX;
                    if (loader != null)
                    {
                        SFXLoader.RegisterSoundToGlobal(EvokeID, loader.LoadSoundFromPath(MainModFile.instance.ImagePath("SFX/FrostEvoke.ogg")), 0.05f);
                    }
                });
        }
    }
}
