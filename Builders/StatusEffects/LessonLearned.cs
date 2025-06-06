using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class LessonLearned : SpirefrostBuilder
    {
        internal static string ID => "Lesson Learned";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectLessonLearned>(ID)
                .WithText("At the end of combat, add +{a}<keyword=attack> to a random <Item> in your deck")
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectLessonLearned>(data =>
                {
                    data.WithSwappable(TryGet<StatusEffectData>("On Card Played Apply Attack To Self"));
                });
        }
    }
}
