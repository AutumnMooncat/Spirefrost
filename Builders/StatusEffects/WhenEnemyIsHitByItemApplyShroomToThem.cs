using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenEnemyIsHitByItemApplyShroomToThem : SpirefrostBuilder
    {
        internal static string ID => "When Enemy Is Hit By Item Apply Shroom To Them";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("When Enemy Is Hit By Item Apply Demonize To Them", ID)
                .WithText("When an enemy is hit with an <Item>, apply <{a}><keyword=shroom> to them")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenUnitIsHit>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Shroom");
                });
        }
    }
}
