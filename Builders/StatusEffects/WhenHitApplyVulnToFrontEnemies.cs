using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects.IconEffects;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhenHitApplyVulnToFrontEnemies : SpirefrostBuilder
    {
        internal static string ID => "When Hit Apply Vuln To Front Enemies";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXToFrontEnemiesWhenHit>(ID)
                .WithText($"When hit, apply <{{a}}>{MakeKeywordInsert(VulnerableKeyword.FullID)} to enemies in front")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXToFrontEnemiesWhenHit>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(Vulnerable.ID);
                    data.applyConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintCanBeHit>()
                    };
                    data.noTargetType = NoTargetType.NoTargetForStatus;
                    data.noTargetTypeArgs = new string[] { MakeSpriteInsert(VulnerableIcon.SpriteID) };
                });
        }
    }
}
