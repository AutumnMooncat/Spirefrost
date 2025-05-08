using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Cards.Items;

namespace Spirefrost.Builders.StatusEffects
{
    internal class TriggerWhenReptoDaggerPlayed : SpirefrostBuilder
    {
        internal static string ID => "Trigger When Repto Dagger Played";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectApplyXWhenAnyCardIsPlayed>(ID)
                .WithText($"Trigger when {MakeCardInsert(ReptoDagger.FullID)} is played")
                .WithIsReaction(true)
                .WithCanBeBoosted(false)
                .WithStackable(false)
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXWhenAnyCardIsPlayed>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>("Trigger (High Prio)");
                    data.eventPriority = -99;
                    data.descColorHex = "F99C61";
                    data.targetPlayedCard = false;
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self;
                    data.triggerConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintIsSpecificCard>(t => t.allowedCards = new CardData[] { TryGet<CardData>(ReptoDagger.ID) })
                    };
                });
        }
    }
}
