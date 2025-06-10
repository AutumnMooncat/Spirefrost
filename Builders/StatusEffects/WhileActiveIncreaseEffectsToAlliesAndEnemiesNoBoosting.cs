using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhileActiveIncreaseEffectsToAlliesAndEnemiesNoBoosting : SpirefrostBuilder
    {
        internal static string ID => "While Active Increase Effects To Allies & Enemies (Not Boostable)";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("While Active Increase Effects To Allies & Enemies", ID)
                .WithText("While active, boost the effects of all allies and enemies by {a}")
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectWhileActiveX>(data =>
                {
                    data.affectOthersWithSameEffect = false;
                });
        }
    }
}
