using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class InstantEqualizeShellSpice : SpirefrostBuilder
    {
        internal static string ID => "Instant Equalize Shell Spice";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectInstantEqualize>(ID)
                .WithText("Set <keyword=shell> and <keyword=spice> to the higher of the two")
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantEqualize>(data =>
                {
                    data.effectsToEqualize = new StatusEffectData[]
                    {
                        TryGet<StatusEffectData>("Shell"),
                        TryGet<StatusEffectData>("Spice")
                    };
                });
        }
    }
}
