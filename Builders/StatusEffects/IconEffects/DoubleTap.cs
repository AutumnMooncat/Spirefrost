using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using Spirefrost.StatusEffects;
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
                .Create<StatusEffectSTSDoubleTap>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectSTSDoubleTap>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("MultiHit");
                })
                .Subscribe_WithStatusIcon(DoubleTapIcon.ID);
        }
    }
}
