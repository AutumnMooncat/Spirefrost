using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects.IconEffects
{
    internal class ExtraCounter : SpirefrostBuilder
    {
        internal static string ID => "Extra Counter";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectExtraCounter>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(false)
                .WithIsStatus(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectExtraCounter>(data =>
                {
                    
                });
        }
    }
}
