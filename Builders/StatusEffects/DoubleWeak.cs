using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects.IconEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class DoubleWeak : SpirefrostBuilder
    {
        internal static string ID => "Double STS Weak";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Double Overload", ID)
                .WithText($"Double the target's {MakeKeywordInsert(WeakKeyword.FullID)}")
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantDoubleX>(data =>
                {
                    data.statusToDouble = TryGet<StatusEffectData>(Weak.ID);
                });
        }
    }
}
