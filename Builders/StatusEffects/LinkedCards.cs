using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Cards.Items;
using Spirefrost.Builders.Keywords;
using Spirefrost.StatusEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class LinkedDagger : SpirefrostBuilder
    {
        internal static string ID => "Linked Dagger";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectLinkedCard>(ID)
                .WithText($"{MakeKeywordInsert(LinkedKeyword.FullID)}: {{a}} {{0}}{{!{{a}}|1= |@=<s >!}}")
                .WithTextInsert(MakeCardInsert(ReptoDagger.FullID))
                .WithCanBeBoosted(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectLinkedCard>(data =>
                {
                    data.linkedCard = TryGet<CardData>(ReptoDagger.ID);
                });
        }
    }
}
