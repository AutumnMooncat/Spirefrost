using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects.TraitEffects
{
    internal class CustomBombardEffect : SpirefrostBuilder
    {
        internal static string ID => "Custom Bombard";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectBombardCustom>(ID)
                .WithCanBeBoosted(false)
                .WithStackable(true)
                .SubscribeToAfterAllBuildEvent<StatusEffectBombardCustom>(data =>
                {
                    data.hitFriendlyChance = 0f;
                    data.maxFrontTargets = 2;
                    data.targetCountRange = new UnityEngine.Vector2Int(3, 3);
                });
        }
    }
}
