using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects.IconEffects;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnCardPlayedChannelLightning : SpirefrostBuilder
    {
        internal static string ID => "On Card Played Channel Lightning";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Card Played Apply Block To Self", ID)
                .WithCanBeBoosted(true)
                .WithStackable(true)
                .WithText($"{MakeKeywordInsert(ChannelKeyword.FullID)} <{{a}}>{MakeKeywordInsert(LightningKeyword.FullID)}")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(InstantChannelLightning.ID);
                    data.targetConstraints = StatusEffectOrb.OrbConstraints();
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
                .WithCanBeBoosted(true)
                .WithStackable(true)
                .WithText($"{MakeKeywordInsert(ChannelKeyword.FullID)} <{{a}}>{MakeKeywordInsert(FrostKeyword.FullID)}")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(InstantChannelFrost.ID);
                    data.targetConstraints = StatusEffectOrb.OrbConstraints();
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
                .WithCanBeBoosted(true)
                .WithStackable(true)
                .WithText($"{MakeKeywordInsert(ChannelKeyword.FullID)} <{{a}}>{MakeKeywordInsert(DarkKeyword.FullID)}")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(InstantChannelDark.ID);
                    data.targetConstraints = StatusEffectOrb.OrbConstraints();
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
                .WithCanBeBoosted(true)
                .WithStackable(true)
                .WithText($"{MakeKeywordInsert(ChannelKeyword.FullID)} <{{a}}>{MakeKeywordInsert(PlasmaKeyword.FullID)}")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(InstantChannelPlasma.ID);
                    data.targetConstraints = StatusEffectOrb.OrbConstraints();
                });
        }
    }

    internal class WhenDeployedChannelDark : SpirefrostBuilder
    {
        internal static string ID => "When Deployed Channel Dark";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Deployed Apply Block To Self", ID)
                .WithText($"When deployed, {MakeKeywordInsert(ChannelKeyword.FullID)} <{{a}}>{MakeKeywordInsert(DarkKeyword.FullID)}")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenDeployed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(InstantChannelDark.ID);
                    data.targetConstraints = StatusEffectOrb.OrbConstraints();
                });
        }
    }

    internal class InstantChannelLightning : SpirefrostBuilder
    {
        internal static string ID => "Instant Channel Lightning";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectInstantChannel>(ID)
                .WithText($"{MakeKeywordInsert(ChannelKeyword.FullID)} <{{a}}>{MakeKeywordInsert(LightningKeyword.FullID)}")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantChannel>(data =>
                {
                    data.orbToChannel = (StatusEffectOrb)TryGet<StatusEffectData>(LightningOrb.ID);
                    data.orbAmount = LightningOrb.ApplyAmount;
                    data.targetConstraints = StatusEffectOrb.OrbConstraints();
                });
        }
    }

    internal class InstantChannelFrost : SpirefrostBuilder
    {
        internal static string ID => "Instant Channel Frost";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectInstantChannel>(ID)
                .WithText($"{MakeKeywordInsert(ChannelKeyword.FullID)} <{{a}}>{MakeKeywordInsert(FrostKeyword.FullID)}")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantChannel>(data =>
                {
                    data.orbToChannel = (StatusEffectOrb)TryGet<StatusEffectData>(FrostOrb.ID);
                    data.orbAmount = FrostOrb.ApplyAmount;
                    data.targetConstraints = StatusEffectOrb.OrbConstraints();
                });
        }
    }

    internal class InstantChannelDark : SpirefrostBuilder
    {
        internal static string ID => "Instant Channel Dark";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectInstantChannel>(ID)
                .WithText($"{MakeKeywordInsert(ChannelKeyword.FullID)} <{{a}}>{MakeKeywordInsert(DarkKeyword.FullID)}")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantChannel>(data =>
                {
                    data.orbToChannel = (StatusEffectOrb)TryGet<StatusEffectData>(DarkOrb.ID);
                    data.orbAmount = DarkOrb.ApplyAmount;
                    data.targetConstraints = StatusEffectOrb.OrbConstraints();
                });
        }
    }

    internal class InstantChannelPlasma : SpirefrostBuilder
    {
        internal static string ID => "Instant Channel Plasma";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectInstantChannel>(ID)
                .WithText($"{MakeKeywordInsert(ChannelKeyword.FullID)} <{{a}}>{MakeKeywordInsert(PlasmaKeyword.FullID)}")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantChannel>(data =>
                {
                    data.orbToChannel = (StatusEffectOrb)TryGet<StatusEffectData>(PlasmaOrb.ID);
                    data.orbAmount = PlasmaOrb.ApplyAmount;
                    data.targetConstraints = StatusEffectOrb.OrbConstraints();
                });
        }
    }
}
