using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects.TraitEffects
{
    internal class RetainEffect : SpirefrostBuilder
    {
        internal static string ID => "STS Retain";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectRetain>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(true);
        }
    }
}
