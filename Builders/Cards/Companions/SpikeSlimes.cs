﻿using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.DefectUnits)]
    internal class SpikeSlime : SpirefrostBuilder
    {
        internal static string ID => "spikeslime";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Spike Slime")
                .SetSprites("Units/SpikeSlime.png", "Units/SpikeSlimeBG.png")
                .SetStats(6, 2, 3)
                .WithValue(50)
                .SetTraits(TStack("Barrage", 1))
                .WithEyes(FullID, (0.65f, 0.7f, 2.5f, 2.25f, 5f))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(AtHalfHeathDivide.ID, 1)
                    };
                });
        }
    }

    internal class SpikeSlime2 : SpirefrostBuilder
    {
        internal static string ID => "spikeslime2";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Spike Slime")
                .SetSprites("Units/SpikeSlime2.png", "Units/SpikeSlimeBG.png")
                .SetStats(6, 2, 3)
                .WithValue(50)
                .SetTraits(TStack("Barrage", 1))
                .WithEyes(FullID, (0.35f, 0.575f, 1.75f, 1.75f, 5f))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(AtHalfHeathDivide.ID, 1)
                    };
                });
        }
    }

    internal class SpikeSlime3 : SpirefrostBuilder
    {
        internal static string ID => "spikeslime3";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Spike Slime")
                .SetSprites("Units/SpikeSlime3.png", "Units/SpikeSlimeBG.png")
                .SetStats(6, 2, 3)
                .WithValue(50)
                .SetTraits(TStack("Barrage", 1))
                .WithEyes(FullID, (0.175f, 0.275f, 1.35f, 1.35f, 5f))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(AtHalfHeathDivide.ID, 1)
                    };
                });
        }
    }
}
