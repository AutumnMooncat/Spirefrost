using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnKillReduceMaxCounter : SpirefrostBuilder
    {
        internal static string ID => "On Kill Reduce Max Counter";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Kill Apply Attack To Self", ID)
                .WithText("On kill, reduce <keyword=counter> by <{a}>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnKill>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Reduce Max Counter");
                });
        }
    }
}
