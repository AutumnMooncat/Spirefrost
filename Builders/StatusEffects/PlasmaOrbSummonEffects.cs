using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Cards.Summons;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnTurnSummonPlasmaOrb : SpirefrostBuilder
    {
        internal static string ID => "On Turn Summon Plasma Orb";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Turn Summon Bootleg Copy of RandomEnemy", ID)
                .WithText("Summon {0}")
                .WithTextInsert(MakeCardInsert(PlasmaOrb.FullID))
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnTurn>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>(InstantSummonPlasmaOrbAtAppliersPosition.ID);
                });
        }
    }

    internal class InstantSummonPlasmaOrbAtAppliersPosition : SpirefrostBuilder
    {
        internal static string ID => "Instant Summon Plasma Orb At Appliers Position";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Instant Summon Bootleg Copy At Appliers Position", ID)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantSummon>(data =>
                {
                    data.withEffects = new StatusEffectData[0];
                    data.summonCopy = false;
                    data.targetSummon = (StatusEffectSummon)TryGet<StatusEffectData>(SummonPlasmaOrb.ID);
                });
        }
    }

    internal class SummonPlasmaOrb : SpirefrostBuilder
    {
        internal static string ID => "Summon Plasma Orb";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Summon Beepop", ID)
                .WithText("Summon {0}")
                .WithTextInsert(MakeCardInsert(PlasmaOrb.FullID))
                .SubscribeToAfterAllBuildEvent<StatusEffectSummon>(data =>
                {
                    data.summonCard = TryGet<CardData>(PlasmaOrb.ID);
                });
        }
    }
}
