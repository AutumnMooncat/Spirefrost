using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects.IconEffects;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhileActiveConstrictedToFrontEnemy : SpirefrostBuilder
    {
        internal static string ID => "While Active Constricted To Front Enemy";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("While Active Increase Effects To FrontAlly", ID)
                .WithText($"While active, add <{{a}}>{MakeKeywordInsert(ConstrictedKeyword.FullID)} to the front enemy")
                .WithStackable(true)
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectWhileActiveX>(data => {
                    data.effectToApply = TryGet<StatusEffectData>(Constricted.ID);
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.FrontEnemy;
                });
        }
    }
}
