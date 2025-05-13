using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhileInHandIncreaseAllShroomApplied : SpirefrostBuilder
    {
        internal static string ID => "While In Hand Increase All Shroom Applied";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectAffectAllXAppliedExtras>(ID)
                .WithText("While in hand, increase all <keyword=shroom> applied to enemies by <{a}>")
                .WithCanBeBoosted(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectAffectAllXAppliedExtras>(data =>
                {
                    data.effectToAffect = TryGet<StatusEffectData>("Shroom");
                    data.location = StatusEffectAffectAllXAppliedExtras.LocationRequirement.Hand;
                    data.multiplyBy = 1f;
                    data.add = 1;
                    data.targetCanBeFriendly = false;
                });
        }
    }
}
