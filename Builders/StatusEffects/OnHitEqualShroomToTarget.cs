using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnHitEqualShroomToTarget : SpirefrostBuilder
    {
        internal static string ID => "On Hit Equal Shroom To Target";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Hit Equal Overload To Target", ID)
                .WithText("Apply <keyword=shroom> equal to damage dealt")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnHit>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Shroom");
                    data.noTargetType = NoTargetType.NoTargetForStatus;
                    data.noTargetTypeArgs = new string[] { "<sprite name=shroom>" };
                });
        }
    }
}
