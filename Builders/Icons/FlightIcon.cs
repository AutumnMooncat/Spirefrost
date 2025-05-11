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
                .WithApplyVFX(MainModFile.instance.ImagePath("VFX/Flight.png"))
                .WithApplySFX(MainModFile.instance.ImagePath("SFX/Flight.ogg"));
        }
    }
}
