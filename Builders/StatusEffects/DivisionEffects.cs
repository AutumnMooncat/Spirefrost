using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Cards.Companions;
using Spirefrost.Builders.Keywords;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class InstantDivide : SpirefrostBuilder
    {
        internal static string ID => "Instant Divide";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectDivision>(ID)
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectDivision>(data =>
                {
                    data.profiles = new StatusEffectDivision.Profile[] {
                        new StatusEffectDivision.Profile()
                        {
                            cardName = SpikeSlime.FullID,
                            changeToCardName = SpikeSlime2.FullID
                        },
                        new StatusEffectDivision.Profile()
                        {
                            cardName = SpikeSlime2.FullID,
                            changeToCardName = SpikeSlime3.FullID
                        }
                    };
                });
        }
    }

    internal class AtHalfHeathDivide : SpirefrostBuilder
    {
        internal static string ID => "At Half Health Divide";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXAtHalfHealth>(ID)
                .WithCanBeBoosted(false)
                .WithText($"{MakeKeywordInsert(DivideKeyword.FullID)} at half <keyword=health>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXAtHalfHealth>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.effectToApply = TryGet<StatusEffectData>(InstantDivide.ID);
                });
        }
    }
}
