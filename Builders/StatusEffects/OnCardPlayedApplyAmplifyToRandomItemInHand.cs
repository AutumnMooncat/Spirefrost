using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects.IconEffects;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnCardPlayedApplyAmplifyToRandomItemInHand : SpirefrostBuilder
    {
        internal static string ID => "On Card Played Apply Amplify To Random Item In Hand";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Card Played Add Zoomlin To Random Card In Hand", ID)
                .WithText($"Apply <{{a}}>{MakeKeywordInsert(AmplifyKeyword.FullID)} to a random <Item> in your hand")
                .WithCanBeBoosted(true)
                .WithStackable(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnCardPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(Amplify.ID);
                    data.applyConstraints = new TargetConstraint[]
                    {
                        ScriptableObject.CreateInstance<TargetConstraintCanBeBoosted>(),
                        ScriptableObject.CreateInstance<TargetConstraintIsItem>()
                    };
                    data.noTargetType = NoTargetType.NoTargetForStatus;
                    data.noTargetTypeArgs = new string[] { MakeSpriteInsert(AmplifyIcon.SpriteID) };
                    data.WithSwappable(TryGet<StatusEffectData>("On Card Played Boost To Allies & Enemies"));
                });
        }
    }
}
