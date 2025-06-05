using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects.TraitEffects
{
    internal class FocusEffect : SpirefrostBuilder
    {
        internal static string ID => "STS Focus";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectSTSFocus>(ID)
                .WithCanBeBoosted(true)
                .WithStackable(true);
        }
    }
}
