﻿using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.IroncladUnits)]
    internal class Reptomancer : SpirefrostBuilder
    {
        internal static string ID => "reptomancer";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Reptomancer")
                .SetSprites("Units/Reptomancer.png", "Units/ReptomancerBG.png")
                .SetStats(8, 4, 0)
                .WithValue(50)
                .WithEyes(FullID, (0.0f, 1.6f, 0.85f, 0.85f, -10f))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(LinkedDagger.ID, 2),
                        SStack(TriggerWhenReptoDaggerPlayed.ID, 1)
                    };
                });
        }
    }
}
