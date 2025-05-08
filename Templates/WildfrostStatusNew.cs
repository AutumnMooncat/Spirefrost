using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Templates
{
    internal class WildfrostStatusNew : SpirefrostBuilder
    {
        internal static string ID => null;

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectData>(ID)
                .WithText("")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectData>(data =>
                {
                    
                });
        }
    }
}
