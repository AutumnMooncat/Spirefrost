﻿using Deadpan.Enums.Engine.Components.Modding;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Clunkers
{
    [ToPoolList(PoolListType.Items)]
    internal class SpireSpear : SpirefrostBuilder
    {
        internal static string ID => "spirespear";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Spire Spear")
                .SetSprites("Units/SpireSpear.png", "Units/SpireSpearBG.png")
                .SetStats(null, 1, 0)
                .WithCardType("Clunker")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Scrap", 3),
                        SStack("Bonus Damage Equal To Scrap On Board", 1),
                        SStack("When Hit Trigger To Self", 1)
                    };
                });
        }
    }
}
