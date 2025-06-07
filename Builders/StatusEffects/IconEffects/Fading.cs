using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using Spirefrost.StatusEffects;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.StatusEffects.IconEffects
{
    internal class Fading : SpirefrostBuilder
    {
        internal static string ID => "STS Fading";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectSTSFading>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectSTSFading>(data =>
                {
                    data.preventDeath = true;
                    /*data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintIsUnit>(t => {
                            t.mustBeMiniboss = true;
                            t.not = true;
                        })
                    };*/
                })
                .Subscribe_WithStatusIcon(FadingIcon.ID);
        }
    }
}
