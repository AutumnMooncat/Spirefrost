﻿using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.Units)]
    internal class FungiBeast : SpirefrostBuilder
    {
        internal static string ID => "fungi";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Fungi Beast")
                .SetSprites("Units/Fungi.png", "Units/FungiBG.png")
                .SetStats(4, 1, 3)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(WhenHitApplyVulnToFrontEnemies.ID, 1)
                    };
                });
        }
    }
}
