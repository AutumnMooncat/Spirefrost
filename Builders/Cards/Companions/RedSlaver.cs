﻿using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using Spirefrost.Builders.StatusEffects.IconEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.Units)]
    internal class RedSlaver : SpirefrostBuilder
    {
        internal static string ID => "redslaver";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Red Slaver")
                .SetSprites("Units/Slaver.png", "Units/SlaverBG.png")
                .SetStats(6, 3, 3)
                .WithValue(50)
                .SetTraits(TStack("Longshot", 1))
                .WithEyes(FullID, (0.475f, 0.75f, 0.45f, 0.45f, -35f))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Demonize", 1)
                    };
                });
        }
    }
}
