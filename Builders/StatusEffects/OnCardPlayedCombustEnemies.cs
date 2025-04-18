using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnCardPlayedCombustEnemies : SpirefrostBuilder
    {
        internal static string ID => "On Card Played Combust Enemies";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Card Played Damage To Self", ID)
                .WithText("Deal <{a}> damage to all enemies")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Enemies;
                });
        }
    }
}
