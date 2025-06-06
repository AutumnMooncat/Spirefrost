using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
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
                .WithText($"Deal additional damage equal to {MakeKeywordInsert(SkillKeyword.FullID)}<s >in hand")
                .SubscribeToAfterAllBuildEvent<StatusEffectBonusDamageEqualToX>(data =>
                {
                    ScriptableSkillsInHand skills = ScriptableObject.CreateInstance<ScriptableSkillsInHand>();
                    data.scriptableAmount = skills;
                    data.WithSwappable(TryGet<StatusEffectData>("On Hit Damage Damaged Target"), null, new Vector2Int(3, 3));
                });
        }
    }
}
