using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;
using UnityEngine;

namespace Spirefrost.Builders.StatusEffects
{
    internal class BonusShellEqualToSpice : SpirefrostBuilder
    {
        internal static string ID => "Bonus Shell Equal To Spice";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectAffectAllXAppliedExtras>(ID)
                .WithText("Gain additional <keyword=shell> equal to <keyword=spice>")
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectAffectAllXAppliedExtras>(data =>
                {
                    data.applierMustBeSelf = true;
                    ScriptableCurrentStatus scriptable = ScriptableObject.CreateInstance<ScriptableCurrentStatus>();
                    scriptable.statusType = TryGet<StatusEffectData>("Spice").type;
                    data.scriptableAmount = scriptable;
                    data.effectToAffect = TryGet<StatusEffectData>("Shell");
                });
        }
    }
}
