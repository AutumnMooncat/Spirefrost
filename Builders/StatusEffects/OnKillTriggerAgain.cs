using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnKillTriggerAgain : SpirefrostBuilder
    {
        internal static string ID => "On Kill Trigger Again";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Kill Apply Gold To Self", ID)
                .WithText("On kill, trigger again")
                .WithIsReaction(true)
                .WithCanBeBoosted(false)
                .WithStackable(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnKill>(data =>
                {
                    data.textOrder = 1;
                    data.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
                    data.eventPriority = -99;
                    data.descColorHex = "F99C61";
                });
        }
    }
}
