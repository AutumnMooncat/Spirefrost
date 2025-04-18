using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenDestroyedTriggerToAllyBehind : SpirefrostBuilder
    {
        internal static string ID => "When Destroyed Trigger To AllyBehind";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Destroyed Trigger To Allies", ID)
                .WithText("When destroyed, trigger ally behind")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenDestroyed>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyBehind;
                });
        }
    }
}
