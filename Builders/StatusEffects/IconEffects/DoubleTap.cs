using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.StatusEffects.IconEffects
{
    internal class DoubleTap : SpirefrostBuilder
    {
        internal static string ID => "STS Double Tap";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectDoubleTap>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectDoubleTap>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("MultiHit");
                })
                .Subscribe_WithStatusIcon(DoubleTapIcon.ID);
        }
    }
}
