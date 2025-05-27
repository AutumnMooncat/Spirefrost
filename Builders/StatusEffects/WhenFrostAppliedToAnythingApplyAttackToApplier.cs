using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using Spirefrost.Builders.Keywords;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenFrostAppliedToAnythingApplyAttackToApplier : SpirefrostBuilder
    {
        internal static string ID => "When Frost Applied To Anything Apply Attack To Applier";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXToApplierWhenYAppliedTo>(ID)
                .WithText($"Whenever anything is <keyword=frost>'d, add <+{{a}}><keyword=attack> to the applier")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXToApplierWhenYAppliedTo>(data =>
                {
                    data.whenAppliedToFlags = StatusEffectApplyX.ApplyToFlags.Self | StatusEffectApplyX.ApplyToFlags.Allies | StatusEffectApplyX.ApplyToFlags.Enemies;
                    data.effectToApply = TryGet<StatusEffectData>("Increase Attack");
                    data.whenAppliedTypes = new string[]
                    {
                        "frost"
                    };
                });
        }
    }
}
