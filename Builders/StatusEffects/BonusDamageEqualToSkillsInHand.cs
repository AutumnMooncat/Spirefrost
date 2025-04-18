using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class BonusDamageEqualToSkillsInHand : SpirefrostBuilder
    {
        internal static string ID => "Bonus Damage Equal To Skills In Hand";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Bonus Damage Equal To Gold Factor 0.02", ID)
                .WithText("Deal additional damage equal to <Items> in hand without <keyword=attack>")
                .SubscribeToAfterAllBuildEvent<StatusEffectBonusDamageEqualToX>(data =>
                {
                    ScriptableSkillsInHand skills = ScriptableObject.CreateInstance<ScriptableSkillsInHand>();
                    data.scriptableAmount = skills;
                });
        }
    }
}
