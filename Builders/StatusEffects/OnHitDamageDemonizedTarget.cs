using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects.IconEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnHitDamageDemonizedTarget : SpirefrostBuilder
    {
        internal static string ID => "On Hit Damage Demonized Target";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Hit Damage Snowed Target", ID)
                .WithTextInsert("<keyword=demonize>")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnHit>(data =>
                {
                    data.applyConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintHasStatus>(t => t.status = TryGet<StatusEffectData>("Demonize"))
                    };
                });
        }
    }
}
