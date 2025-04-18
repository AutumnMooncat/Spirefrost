using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenEnemyIsHitByItemReduceTheirHealth : SpirefrostBuilder
    {
        internal static string ID => "When Enemy Is Hit By Item Reduce Their Health";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Enemy Is Hit By Item Apply Demonize To Them", ID)
                .WithText("When an enemy is hit with an <Item>, reduce their <keyword=health> by <{a}>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenUnitIsHit>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Reduce Max Health");
                });
        }
    }
}
