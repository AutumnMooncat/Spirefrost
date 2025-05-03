using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Traits;

namespace Spirefrost.Builders.StatusEffects
{
    internal class TemporaryFocus : SpirefrostBuilder
    {
        internal static string ID => "Temporary Focus";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Temporary Pigheaded", ID)
                .WithCanBeBoosted(true)
                .WithStackable(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectTemporaryTrait>(data =>
                {
                    data.trait = TryGet<TraitData>(FocusTrait.ID);
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintIsUnit>()
                    };
                });
        }
    }
}
