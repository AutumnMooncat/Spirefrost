using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenCardDestroyedRecoverHealthToSelf : SpirefrostBuilder
    {
        internal static string ID => "When Card Destroyed, Recover Health To Self";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Card Destroyed, Gain Attack", ID)
                .WithText("When a card is destroyed, restore <{a}><keyword=health>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenCardDestroyed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Heal (No Ping)");
                });
        }
    }
}
