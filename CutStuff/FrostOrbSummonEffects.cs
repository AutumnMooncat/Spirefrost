using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Cards.Summons;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnTurnSummonFrostOrb : SpirefrostBuilder
    {
        internal static string ID => "On Turn Summon Frost Orb";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Turn Summon Bootleg Copy of RandomEnemy", ID)
                .WithText("Summon {0}")
                .WithTextInsert(MakeCardInsert(FrostOrb.FullID))
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnTurn>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>(InstantSummonFrostOrbAtAppliersPosition.ID);
                });
        }
    }

    internal class InstantSummonFrostOrbAtAppliersPosition : SpirefrostBuilder
    {
        internal static string ID => "Instant Summon Frost Orb At Appliers Position";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Instant Summon Bootleg Copy At Appliers Position", ID)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantSummon>(data =>
                {
                    data.withEffects = new StatusEffectData[0];
                    data.summonCopy = false;
                    data.targetSummon = (StatusEffectSummon)TryGet<StatusEffectData>(SummonFrostOrb.ID);
                });
        }
    }

    internal class SummonFrostOrb : SpirefrostBuilder
    {
        internal static string ID => "Summon Frost Orb";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Summon Beepop", ID)
                .WithText("Summon {0}")
                .WithTextInsert(MakeCardInsert(FrostOrb.FullID))
                .SubscribeToAfterAllBuildEvent<StatusEffectSummon>(data =>
                {
                    data.summonCard = TryGet<CardData>(FrostOrb.ID);
                });
        }
    }
}
