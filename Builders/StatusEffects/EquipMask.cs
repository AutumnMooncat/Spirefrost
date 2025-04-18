using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class EquipMask : SpirefrostBuilder
    {
        internal static string ID => "STS Equip Mask";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectEquipMask>(ID)
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectEquipMask>(data =>
                {

                });
        }
    }
}
