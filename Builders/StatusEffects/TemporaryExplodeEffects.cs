using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class TemporaryExplode : SpirefrostBuilder
    {
        internal static string ID => "Temporary Explode";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Temporary Pigheaded", ID)
                .WithStackable(true)
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectTemporaryTrait>(data =>
                {
                    data.trait = TryGet<TraitData>("Explode");
                });
        }
    }

    internal class WhileActiveExplodeToAllies : SpirefrostBuilder
    {
        internal static string ID => "While Active Explode To Allies";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("While Active Pigheaded To Enemies", ID)
                .WithText("While active, add <keyword=explode {a}> to all allies")
                .WithStackable(true)
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectWhileActiveX>(data => {
                    data.effectToApply = TryGet<StatusEffectData>(TemporaryExplode.ID);
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Allies;
                });
        }
    }
}
