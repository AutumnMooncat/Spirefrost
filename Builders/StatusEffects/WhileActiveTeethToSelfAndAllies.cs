using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhileActiveTeethToSelfAndAllies : SpirefrostBuilder
    {
        internal static string ID => "While Active Teeth To Self & Allies";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("While Active Teeth To Allies", ID)
                .WithText("While active, add <{a}><keyword=teeth> to self and all allies")
                .SubscribeToAfterAllBuildEvent<StatusEffectWhileActiveX>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self | StatusEffectApplyX.ApplyToFlags.Allies;
                });
        }
    }
}
