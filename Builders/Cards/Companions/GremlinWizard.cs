﻿using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using Spirefrost.Builders.Traits;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.DefectUnits)]
    internal class GremlinWizard : SpirefrostBuilder
    {
        internal static string ID => "gremlinwizard";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Gremlin Wizard")
                .SetSprites("Units/GremlinWizard.png", "Units/GremlinWizardBG.png")
                .SetStats(5, 10, 10)
                .WithValue(50)
                .WithEyes(FullID, 
                (0.1f, 1.95f, 0.5f, 0.5f, -10f),
                (0.35f, 1.9f, 0.5f, 0.5f, -10f))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(WhenAllyAttacksCountDown.ID, 1)
                    };
                    data.traits = new System.Collections.Generic.List<CardData.TraitStacks>
                    {
                        TStack(FocusTrait.ID, 2)
                    };
                });
        }
    }
}
