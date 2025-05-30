﻿using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects.IconEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.IroncladUnits)]
    internal class Darkling : SpirefrostBuilder
    {
        internal static string ID => "darkling";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Darkling")
                .SetSprites("Units/Darkling.png", "Units/DarklingBG.png")
                .SetStats(4, 2, 5)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(Regrow.ID, 2),
                        SStack("When Card Destroyed, Gain Attack & Health", 1)
                    };
                });
        }
    }
}
