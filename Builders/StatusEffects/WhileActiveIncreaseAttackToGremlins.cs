using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Cards.Companions;
using Spirefrost.Builders.Cards.Items;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhileActiveIncreaseAttackToGremlins : SpirefrostBuilder
    {
        internal static string ID => "While Active Increase Attack To Gremlins";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("While Active Increase Attack To Allies", ID)
                .WithText("While active, add <+{a}><keyword=attack> to all <Gremlin> cards")
                .SubscribeToAfterAllBuildEvent<StatusEffectWhileActiveX>(data =>
                {
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.Self | StatusEffectApplyX.ApplyToFlags.Allies | StatusEffectApplyX.ApplyToFlags.Hand;
                    data.applyConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintIsSpecificCard>(t => t.allowedCards = new CardData[]
                        {
                            TryGet<CardData>(FatGremlin.ID),
                            TryGet<CardData>(MadGremlin.ID),
                            TryGet<CardData>(ShieldGremlin.ID),
                            TryGet<CardData>(SneakyGremlin.ID),
                            TryGet<CardData>(GremlinWizard.ID),
                            TryGet<CardData>(GremlinNob.ID),
                            TryGet<CardData>(GremlinLeader.ID),
                            TryGet<CardData>(GremlinHorn.ID),
                            TryGet<CardData>(GremlinVisage.ID),
                        })
                    };
                    data.WithSwappable(TryGet<StatusEffectData>("While Active Increase Attack To Allies"));
                });
        }
    }
}
