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
                .Create<StatusEffectInstanceEqualize>(ID)
                .WithText("Set <keyword=Shell> and <keyword=Spice> to the higher of the two")
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstanceEqualize>(data =>
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
