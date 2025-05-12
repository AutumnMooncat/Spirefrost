using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using UnityEngine;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.Icons
{
    internal class FlightIcon : SpirefrostBuilder
    {
        internal static string ID => "STS Flight Icon";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static string SpriteID => "spirefrost.stsflight";

        internal static object GetBuilder()
        {
            return new StatusIconBuilder(MainModFile.instance)
                .Create(ID, SpriteID, MainModFile.instance.ImagePath("Icons/FlightIcon.png"))
                .WithIconGroupName(StatusIconBuilder.IconGroups.health)
                .WithTextColour(new Color(0.2471f, 0.1216f, 0.1647f, 1f))
                .WithTextShadow(new Color(1.0f, 1.0f, 1.0f, 1.0f))
                .WithTextboxSprite()
                .WithKeywords(FlightKeyword.ID)
                .WithApplySFX(MainModFile.instance.ImagePath("SFX/Flight.ogg"))
                .FreeModify(icon =>
                {
                    GameObject vfx = new SpirefrostVFXBuilder(MainModFile.instance, "Icons/FlightIcon.png")
                    .WithColorGradient(Color.white, new Color(1, 1, 1, 0.75f), new Color(1, 1, 1, 0))
                    .WithSizeGradient(true, 2f, 3f)
                    .WithDuration(1f)
                    .WithVelocityGradient(new Vector3(0, 5, 0), new Vector3(0, 0, 0))
                    .Build();
                    vfx.RegisterAsApplyEffect(icon.type);
                });
        }
    }
}
