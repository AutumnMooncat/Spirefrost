using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnTurnApplySpiceToSelfAndAllyBehind : SpirefrostBuilder
    {
        internal static string ID => "On Turn Apply Spice To Self & AllyBehind";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Turn Apply Spice To AllyBehind", ID)
                .WithText("Apply <{a}><keyword=spice> to self and ally behind")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnTurn>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self | StatusEffectApplyX.ApplyToFlags.AllyBehind;
                    data.WithSwappable(TryGet<StatusEffectData>("On Turn Apply Spice To Allies"));
                });
        }
    }
}
