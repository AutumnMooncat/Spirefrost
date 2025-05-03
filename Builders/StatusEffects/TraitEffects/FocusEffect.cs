using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects.TraitEffects
{
    internal class FocusEffect : SpirefrostBuilder
    {
        internal static string ID => "STS Focus";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectFocus>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(true);
        }
    }
}
