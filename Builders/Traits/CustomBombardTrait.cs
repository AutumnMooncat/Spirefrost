using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects.TraitEffects;

namespace Spirefrost.Builders.Traits
{
    internal class CustomBombardTrait : SpirefrostBuilder
    {
        internal static string ID => "Bombard";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new TraitDataBuilder(MainModFile.instance)
                .Create(ID)
                .SubscribeToAfterAllBuildEvent(trait =>
                {
                    trait.keyword = TryGet<KeywordData>("bombard");
                    trait.effects = new StatusEffectData[]
                    {
                        TryGet<StatusEffectData>(CustomBombardEffect.ID)
                    };
                    trait.MakeExclusiveWith(TryGet<TraitData>("Aimless"), TryGet<TraitData>("Barrage"), TryGet<TraitData>("Longshot"));
                });
        }
    }
}
