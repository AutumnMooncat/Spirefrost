using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.StatusEffects;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class BonusShellEqualToAttacksInHand : SpirefrostBuilder
    {
        internal static string ID => "Bonus Shell Equal To Attacks In Hand";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectAffectAllXAppliedExtras>(ID)
                .WithText($"Apply additional <keyword=shell> equal to {MakeKeywordInsert(AttackKeyword.FullID)}<s >in hand")
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectAffectAllXAppliedExtras>(data =>
                {
                    data.applierMustBeSelf = true;
                    data.scriptableAmount = ScriptableObject.CreateInstance<ScriptableAttacksInHand>();
                    data.effectToAffect = TryGet<StatusEffectData>("Shell");
                });
        }
    }
}
