﻿using Deadpan.Enums.Engine.Components.Modding;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.StarterItems, 1, 1)]
    internal class Shovel : SpirefrostBuilder
    {
        internal static string ID => "shovel";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Shovel")
                .SetSprites("Items/Shovel.png", "Items/ShovelBG.png")
                .WithValue(30)
                .SetDamage(0)
                .SetAttackEffect(SStack("Snow", 2))
                .SetTraits(TStack("Draw", 1))
                .WithFlavour("Diggy diggy hole");
        }
    }
}
