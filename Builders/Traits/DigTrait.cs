using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects.TraitEffects;

namespace Spirefrost.Builders.Traits
{
    internal class DigTrait : SpirefrostBuilder
    {
        internal static string ID => "Dig";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new TraitDataBuilder(MainModFile.instance)
                .Create(ID)
                .SubscribeToAfterAllBuildEvent(trait =>
                {
                    trait.keyword = TryGet<KeywordData>(DigKeyword.ID);
                    trait.effects = new StatusEffectData[]
                    {
                        TryGet<StatusEffectData>(DigEffect.ID)
                    };
                });
        }
    }
}
