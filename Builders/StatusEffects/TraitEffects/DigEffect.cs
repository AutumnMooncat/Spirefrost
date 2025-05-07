using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects.TraitEffects
{
    internal class DigEffect : SpirefrostBuilder
    {
        internal static string ID => "STS Dig";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Card Played Add Junk To Hand", ID)
                .WithCanBeBoosted(true)
                .WithStackable(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(InstantDigEffect.ID);
                });
        }
    }

    internal class InstantDigEffect : SpirefrostBuilder
    {
        internal static string ID => "Instant Dig";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectInstantPutIntoHand>(ID)
                .WithCanBeBoosted(true)
                .WithStackable(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantPutIntoHand>(data =>
                {
                    data.allowItems = true;
                });
        }
    }
}
