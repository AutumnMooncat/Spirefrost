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

        internal static object GetBuilder()
        {
            return new StatusIconBuilder(MainModFile.instance)
                .CreateCustom(ID, SpriteID, MainModFile.instance.ImagePath("Icons/DarkIcon.png"))
                .WithIconGroupName(LayoutPatch.orbIconGroup)
                .WithTextColour(new Color(0.2471f, 0.1216f, 0.1647f, 1f))
                .WithTextShadow(new Color(1.0f, 1.0f, 1.0f, 1.0f))
                .WithTextboxSprite()
                .WithKeywords(DarkKeyword.ID)
                .FreeModify(action =>
                {
                    action.textElement.outlineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    action.textElement.outlineWidth = 0.2f;
                    action.textElement.fontSharedMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.25f);
                });
        }
    }
}
