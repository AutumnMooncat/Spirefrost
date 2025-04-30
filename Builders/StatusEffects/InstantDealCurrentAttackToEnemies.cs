using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class InstantDealCurrentAttackToEnemies : SpirefrostBuilder
    {
        internal static string ID => " Instant Deal Current Attack To Enemies";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Instant Apply Current Attack To Random Ally", ID)
                .WithText("Deal its <keyword=attack> to all enemies")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXInstant>(data =>
                {
                    data.dealDamage = true;
                    data.doesDamage = true;
                    data.countsAsHit = true;
                    data.canRetaliate = false;
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Enemies;
                    data.effectToApply = null;
                    data.applyConstraints = new TargetConstraint[0];
                });
        }
    }
}
