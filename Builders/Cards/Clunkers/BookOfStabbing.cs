﻿using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.Cards.Clunkers
{
    internal class BookOfStabbing : SpirefrostBuilder
    {
        internal static string ID => "bookofstabbing";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Book of Stabbing")
                .SetSprites("Units/BookOfStabbing.png", "Units/BookOfStabbingBG.png")
                .SetStats(null, null, 0)
                .WithCardType("Clunker")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Scrap", 1),
                        SStack("While Active Frenzy To AlliesInRow", 1)
                    };
                });
        }
    }
}
