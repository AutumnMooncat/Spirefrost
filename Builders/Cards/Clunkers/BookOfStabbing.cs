﻿using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Clunkers
{
    [ToPoolList(PoolListType.Items)]
    internal class BookOfStabbing : SpirefrostBuilder
    {
        internal static string ID => "bookofstabbing";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Book of Stabbing")
                .SetSprites("Units/BookOfStabbing.png", "Units/BookOfStabbingBG.png")
                .SetStats(null, 3, 0)
                .WithCardType("Clunker")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Scrap", 1),
                        SStack(TriggerAgainstWhenDemonizeApplied.ID, 1)
                    };
                });
        }
    }
}
