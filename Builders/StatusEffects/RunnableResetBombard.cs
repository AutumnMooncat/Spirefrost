using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;
using System.Collections;
using System.Linq;

namespace Spirefrost.Builders.StatusEffects
{
    internal class RunnableResetBombard : SpirefrostBuilder
    {
        internal static string ID => "Runnable Reset Bombard";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectInstantRunnable>(ID)
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectInstantRunnable>(data =>
                {
                    data.runnable = Run;
                });
        }

        internal static IEnumerator Run(StatusEffectData data)
        {
            StatusEffectBombard found = data.target.statusEffects.Where(effect => effect is StatusEffectBombard).FirstOrDefault() as StatusEffectBombard;
            if (found)
            {
                yield return found.SetTargets();
            }
        }
    }
}
