﻿using Deadpan.Enums.Engine.Components.Modding;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.Units)]
    internal class ShieldGremlin : SpirefrostBuilder
    {
        internal static string ID => "shieldgremlin";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Shield Gremlin")
                .SetSprites("Units/ShieldGremlin.png", "Units/ShieldGremlinBG.png")
                .SetStats(3, null, 4)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("On Turn Apply Shell To AllyInFrontOf", 2)
                    };
                });
        }
    }
}
