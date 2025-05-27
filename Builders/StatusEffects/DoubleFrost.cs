using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class DoubleFrost : SpirefrostBuilder
    {
        internal static string ID => "Double Frost";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Double Overload", ID)
                .WithText($"Double the target's <keyword=frost>")
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantDoubleX>(data =>
                {
                    data.statusToDouble = TryGet<StatusEffectData>("Frost");
                });
        }
    }
}
