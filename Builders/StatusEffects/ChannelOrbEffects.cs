using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects.IconEffects;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnCardPlayedChannelLightning : SpirefrostBuilder
    {
        internal static string ID => "On Card Played Channel Lightning";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Card Played Apply Block To Self", ID)
                .WithCanBeBoosted(false)
                .WithStackable(false)
                .WithText($"Gain {MakeKeywordInsert(LightningKeyword.FullID)}")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(LightningOrb.ID);
                });
        }
    }

    internal class OnCardPlayedChannelFrost : SpirefrostBuilder
    {
        internal static string ID => "On Card Played Channel Frost";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Card Played Apply Block To Self", ID)
                .WithCanBeBoosted(false)
                .WithStackable(false)
                .WithText($"Gain {MakeKeywordInsert(FrostKeyword.FullID)}")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(FrostOrb.ID);
                });
        }
    }

    internal class OnCardPlayedChannelDark : SpirefrostBuilder
    {
        internal static string ID => "On Card Played Channel Dark";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Card Played Apply Block To Self", ID)
                .WithCanBeBoosted(false)
                .WithStackable(false)
                .WithText($"Gain {MakeKeywordInsert(DarkKeyword.FullID)}")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(DarkOrb.ID);
                });
        }
    }

    internal class OnCardPlayedChannelPlasma : SpirefrostBuilder
    {
        internal static string ID => "On Card Played Channel Plasma";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Card Played Apply Block To Self", ID)
                .WithCanBeBoosted(false)
                .WithStackable(false)
                .WithText($"Gain {MakeKeywordInsert(PlasmaKeyword.FullID)}")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(PlasmaOrb.ID);
                });
        }
    }
}
