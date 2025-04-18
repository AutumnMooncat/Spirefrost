using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using WildfrostHopeMod.VFX;

namespace Spirefrost.Builders.StatusEffects.IconEffects
{
    internal class Weak : SpirefrostBuilder
    {
        internal static string ID => "STS Weak";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectSTSWeakness>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .WithIsStatus(true)
                .WithOffensive(true)
                .Subscribe_WithStatusIcon(WeakIcon.ID);
        }
    }
}
