using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenCardDestroyedDamageEnemies : SpirefrostBuilder
    {
        internal static string ID => "When Card Destroyed, Damage enemies";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXWhenCardDestroyed>(ID)
                .WithText("When a card is destroyed, deal <{a}> damage to all enemies")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenCardDestroyed>(data =>
                {
                    data.dealDamage = true;
                    data.doesDamage = true;
                    data.countsAsHit = true;
                    data.canRetaliate = false;
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Enemies;
                    data.mustBeOnBoard = false;
                });
        }
    }
}
