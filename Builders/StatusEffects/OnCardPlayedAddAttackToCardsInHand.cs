﻿using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnCardPlayedAddAttackToCardsInHand : SpirefrostBuilder
    {
        internal static string ID => "On Card Played Add Attack To Cards In Hand";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Card Played Add Zoomlin To Cards In Hand", ID)
                .WithCanBeBoosted(true)
                .WithStackable(true)
                .WithText("Add <+{a}><keyword=attack> to all cards in your hand")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Increase Attack");
                    data.noTargetType = NoTargetType.NoTargetForStatus;
                    data.noTargetTypeArgs = new string[] { "<sprite name=attack>" };
                });
        }
    }
}
