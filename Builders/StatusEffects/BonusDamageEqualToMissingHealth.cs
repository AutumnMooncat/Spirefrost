using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class BonusDamageEqualToMissingHealth : SpirefrostBuilder
    {
        internal static string ID => "Bonus Damage Equal To Missing Health";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Bonus Damage Equal To Gold Factor 0.02", ID)
                .WithText("Deal additional damage equal to missing <keyword=health>")
                .SubscribeToAfterAllBuildEvent<StatusEffectBonusDamageEqualToX>(data =>
                {
                    data.scriptableAmount = ScriptableObject.CreateInstance<ScriptableMissingHealth>(); ;
                });
        }
    }
}
