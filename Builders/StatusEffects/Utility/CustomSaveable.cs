using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects.Utility
{
    internal class CustomSaveable : SpirefrostBuilder
    {
        internal static string ID => "STS Custom Saveable";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectCustomSaveable>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectCustomSaveable>(data =>
                {

                });
        }
    }
}
