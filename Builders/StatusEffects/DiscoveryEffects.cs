using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Cards.Items;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    /*internal class OnCardPlayedDoDiscoveryToolbox : SpirefrostBuilder
    {
        internal static string ID => "On Card Played Do Discovery Toolbox";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Card Played Add Gearhammer To Hand", ID)
                .WithText("Choose 1 of <{a}> random <Items> to add to your hand")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(DiscoveryToolbox.ID);
                });
        }
    }

    internal class DiscoveryToolbox : SpirefrostBuilder
    {
        internal static string ID => "STS Discovery Toolbox";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectDiscovery>(ID)
                .WithText("")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectDiscovery>(data =>
                {
                    MainModFile.instance.predicateReferences[data.name] = obj => obj is CardData cardData && cardData.IsItem && cardData.name != Toolbox.FullID;
                    data.source = StatusEffectDiscovery.CardSource.Custom;
                    data.title = LocalizationHelper.GetCollection("UI Text", SystemLanguage.English).GetString(SpirefrostStrings.ToolboxTitle);
                });
        }
    }*/

    internal class OnCardPlayedDoDiscoveryGoldenEye : SpirefrostBuilder
    {
        internal static string ID => "On Card Played Do Discovery Golden Eye";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Card Played Add Gearhammer To Hand", ID)
                .WithText("Choose a card in your draw pile to draw")
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(DiscoveryGoldenEye.ID);
                });
        }
    }

    internal class DiscoveryGoldenEye : SpirefrostBuilder
    {
        internal static string ID => "STS Discovery Golden Eye";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectDiscovery>(ID)
                .WithText("")
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectDiscovery>(data =>
                {
                    data.source = StatusEffectDiscovery.CardSource.Draw;
                    data.title = LocalizationHelper.GetCollection("UI Text", SystemLanguage.English).GetString(SpirefrostStrings.GoldenEyeTitle);
                });
        }
    }
}
