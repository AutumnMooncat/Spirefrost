using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnTurnEndConstrictedDamageFrontEnemy : SpirefrostBuilder
    {
        internal static string ID => "On Turn End Constricted Damage Front Enemy";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXAfterTurn>(ID)
                .WithText("While active, deal <{a}> damage to the front enemy each turn")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXAfterTurn>(data =>
                {
                    data.ignoreSilence = false;
                    data.effectToApply = TryGet<StatusEffectData>(ConstrictedDamage.ID);
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.FrontEnemy;
                });
        }
    }

    internal class ConstrictedDamage : SpirefrostBuilder
    {
        internal static string ID => "Constricted Damage";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectInstantDamage>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantDamage>(data =>
                {
                    data.doesDamage = true;
                    data.countsAsHit = true;
                    data.canRetaliate = false;
                    data.damageType = ConstrictedIcon.DamageID;
                });
        }
    }
}
