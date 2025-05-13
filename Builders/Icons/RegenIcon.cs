using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using UnityEngine;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.Icons
{
    internal class RegenIcon : SpirefrostBuilder
    {
        internal static string ID => "STS Regen Icon";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static string SpriteID => "spirefrost.stsregen";

        internal static object GetBuilder()
        {
            return new StatusIconBuilder(MainModFile.instance)
                .Create(ID, SpriteID, MainModFile.instance.ImagePath("Icons/RegenIcon.png"))
                .WithIconGroupName(StatusIconBuilder.IconGroups.health)
                .WithTextColour(new Color(0.2471f, 0.1216f, 0.1647f, 1f))
                .WithTextShadow(new Color(1.0f, 1.0f, 1.0f, 1.0f))
                .WithTextboxSprite()
                .WithKeywords(RegenKeyword.ID)
                .WithApplySFX(MainModFile.instance.ImagePath("SFX/Heal.ogg"))
                .FreeModify(icon =>
                {
                    GameObject vfx = new SpirefrostVFXBuilder(MainModFile.instance, "Icons/RegenIcon.png")
                    .WithColorGradient(Color.white, Color.white, new Color(1, 1, 1, 0))
                    .WithSizeGradient(true, 2f, 3f)
                    .WithDuration(1f)
                    .Build();
                    vfx.RegisterAsApplyEffect(icon.type);
                });
        }
    }
}
