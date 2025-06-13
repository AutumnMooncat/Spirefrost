using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.StatusEffects;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class BonusDamageEqualToSkillsInHand : SpirefrostBuilder
    {
        internal static string ID => "Bonus Damage Equal To Skills In Hand";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectLessJankyBonusDamageEqualToX>(ID)
                .WithText($"Deal additional damage equal to {MakeKeywordInsert(SkillKeyword.FullID)}<s >in hand")
                .SubscribeToAfterAllBuildEvent<StatusEffectLessJankyBonusDamageEqualToX>(data =>
                {
                    ScriptableSkillsInHand skills = ScriptableObject.CreateInstance<ScriptableSkillsInHand>();
                    data.scriptableAmount = skills;
                    data.on = StatusEffectLessJankyBonusDamageEqualToX.On.ScriptableAmount;
                    data.WithSwappable(TryGet<StatusEffectData>("On Hit Damage Damaged Target"), null, new Vector2Int(3, 3));
                });
        }
    }
}
