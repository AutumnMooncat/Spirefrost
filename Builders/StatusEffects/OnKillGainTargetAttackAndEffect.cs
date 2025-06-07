using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnKillGainTargetAttackAndEffect : SpirefrostBuilder
    {
        internal static string ID => "On Kill Gain Target Attack And Effect";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Kill Apply Attack To Self", ID)
                .WithText("On kill, gain the target's <keyword=attack> and effects")
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnKill>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;
                    data.effectToApply = TryGet<StatusEffectData>(InstantGainTargetAttackAndEffect.ID);
                });
        }
    }

    internal class OnKillGainTargetAttack : SpirefrostBuilder
    {
        internal static string ID => "On Kill Gain Target Attack";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Kill Apply Attack To Self", ID)
                .WithText("On kill, gain the target's <keyword=attack>")
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnKill>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Target;
                    data.effectToApply = TryGet<StatusEffectData>(InstantGainTargetAttack.ID);
                });
        }
    }

    internal class InstantGainTargetAttackAndEffect : SpirefrostBuilder
    {
        internal static string ID => "Instant Gain Target Attack And Effect";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectInstantTakeStatsAndEffects>(ID)
                .WithText("Gain the target's <keyword=attack> and effects")
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantTakeStatsAndEffects>(data =>
                {
                    StatusEffectInstantEat reference = TryGet<StatusEffectData>("Eat (Health, Attack & Effects)") as StatusEffectInstantEat;
                    data.illegalEffects = reference.illegalEffects;
                    data.illegalTraits = reference.illegalTraits;
                    data.gainHealth = false;
                });
        }
    }

    internal class InstantGainTargetAttack : SpirefrostBuilder
    {
        internal static string ID => "Instant Gain Target Attack";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectInstantTakeStatsAndEffects>(ID)
                .WithText("Gain the target's <keyword=attack>")
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantTakeStatsAndEffects>(data =>
                {
                    data.gainEffects = false;
                    data.gainHealth = false;
                });
        }
    }
}
