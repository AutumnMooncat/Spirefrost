﻿using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects.IconEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.Units)]
    internal class FatGremlin : SpirefrostBuilder
    {
        internal static string ID => "fatgremlin";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Fat Gremlin")
                .SetSprites("Units/FatGremlin.png", "Units/FatGremlinBG.png")
                .SetStats(2, 1, 2)
                .WithValue(50)
                .WithEyes(FullID, 
                (-0.15f, 0.375f, 0.5f, 0.5f, 0f),
                (0.225f, 0.375f, 0.5f, 0.5f, 0f))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Frost", 1)
                    };
                });
        }
    }
}
