using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using Spirefrost.Builders.StatusEffects.IconEffects;
using System.Collections.Generic;
using UnityEngine;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Leaders
{
    [ToPoolList(PoolListType.Leaders)]
    internal class Watcher : SpirefrostBuilder
    {
        internal static string ID => "watcher";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Watcher")
                .SetSprites("Leaders/Watcher.png", "Leaders/WatcherBG.png")
                .SetStats(7, 4, 5)
                .WithValue(25)
                .WithCardType("Leader")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    CardScriptRunnable watcherScript = ScriptableObject.CreateInstance<CardScriptRunnable>();
                    watcherScript.runnable = card =>
                    {
                        card.GiveUpgrade();
                        int ability = new Vector2Int(0, 6).Random();
                        switch (ability)
                        {
                            // Swivel
                            case 0:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(4, 6);
                                card.SetRandomCounter(5, 5);
                                card.SetRandomPassive(OnCardPlayedAddZoomlinToRandomAttackInHand.ID, 1, 1);
                                break;

                            // Pressure Points
                            case 1:
                                card.SetRandomHealth(5, 7);
                                card.SetRandomDamage(0, 1);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomActive(Mark.ID, 3, 3);
                                break;

                            // Miracle
                            case 2:
                                card.SetRandomHealth(6, 8);
                                card.SetRandomDamage(4, 6);
                                card.SetRandomCounter(6, 7);
                                card.SetRandomPassive(OnCardPlayedAddHolyWaterToHand.ID, 1, 1);
                                break;

                            // Empty Mind
                            case 3:
                                card.traits = new List<CardData.TraitStacks> { TStack("Draw", 2) };
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(4, 5);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomPassive(OnTurnCleanseSelf.ID, 1, 1);
                                break;

                            // Follow Up
                            case 4:
                                card.SetRandomHealth(7, 9);
                                card.SetRandomDamage(3, 3);
                                card.SetRandomCounter(5, 6);
                                card.SetRandomPassive(WhenAttackItemPlayedReduceCounter.ID, 1, 1);
                                break;

                            // Lesson Learned
                            case 5:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(4, 6);
                                card.SetRandomCounter(3, 4);
                                card.SetRandomPassive(LessonLearned.ID, 2, 2);
                                break;

                            // Judgement
                            case 6:
                                card.SetRandomHealth(7, 9);
                                card.SetRandomDamage(4, 6);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomPassive(OnTurnJudgeEnemies.ID, 2, 2);
                                break;
                        }
                    };

                    data.createScripts = new CardScript[]
                    {
                        watcherScript
                    };
                });
        }
    }
}
