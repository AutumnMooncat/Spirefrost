using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Cards.Summons;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnTurnSummonLightningOrb : SpirefrostBuilder
    {
        internal static string ID => "On Turn Summon Lightning Orb";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Turn Summon Bootleg Copy of RandomEnemy", ID)
                .WithText("Summon {0}")
                .WithTextInsert(MakeCardInsert(LightningOrb.FullID))
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnTurn>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>(InstantSummonLightningOrbAtAppliersPosition.ID);
                });
        }
    }

    internal class InstantSummonLightningOrbAtAppliersPosition : SpirefrostBuilder
    {
        internal static string ID => "Instant Summon Lightning Orb At Appliers Position";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Instant Summon Bootleg Copy At Appliers Position", ID)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantSummon>(data =>
                {
                    data.withEffects = new StatusEffectData[0];
                    data.summonCopy = false;
                    data.targetSummon = (StatusEffectSummon)TryGet<StatusEffectData>(SummonLightningOrb.ID);
                });
        }
    }

    internal class SummonLightningOrb : SpirefrostBuilder
    {
        internal static string ID => "Summon Lightning Orb";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Summon Beepop", ID)
                .WithText("Summon {0}")
                .WithTextInsert(MakeCardInsert(LightningOrb.FullID))
                .SubscribeToAfterAllBuildEvent<StatusEffectSummon>(data =>
                {
                    data.summonCard = TryGet<CardData>(LightningOrb.ID);
                });
        }
    }
}
