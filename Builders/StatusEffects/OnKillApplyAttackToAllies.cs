using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnKillApplyAttackToAllies : SpirefrostBuilder
    {
        internal static string ID => "On Kill Apply Attack To Allies";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Kill Apply Attack To Self", ID)
                .WithText("On kill, add <+{a}><keyword=attack> to all allies")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnKill>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
                });
        }
    }
}
