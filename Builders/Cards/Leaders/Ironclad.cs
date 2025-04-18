using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using Spirefrost.Builders.StatusEffects.IconEffects;
using UnityEngine;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Leaders
{
    [ToPoolList(PoolListType.Leaders)]
    internal class Ironclad : SpirefrostBuilder
    {
        internal static string ID => "ironclad";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Ironclad")
                .SetSprites("Leaders/Ironclad.png", "Leaders/IroncladBG.png")
                .SetStats(8, 3, 4)
                .WithValue(25)
                .WithCardType("Leader")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    CardScriptRunnable ironcladScript = ScriptableObject.CreateInstance<CardScriptRunnable>();
                    ironcladScript.runnable = card =>
                    {
                        card.GiveUpgrade();
                        int ability = new Vector2Int(0, 6).Random();
                        switch (ability)
                        {
                            // Feel No Pain
                            case 0:
                                card.SetRandomHealth(7, 9);
                                card.SetRandomDamage(4, 6);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomPassive(WhenCardDestroyedGainShell.ID, 1, 1);
                                break;

                            // Demon Form
                            case 1:
                                card.SetRandomHealth(6, 8);
                                card.SetRandomDamage(2, 3);
                                card.SetRandomCounter(4, 4);
                                card.SetRandomPassive("On Turn Apply Attack To Self", 1, 1);
                                break;

                            // Bash
                            case 2:
                                card.SetRandomHealth(8, 11);
                                card.SetRandomDamage(4, 6);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomActive(Vulnerable.ID, 2, 3);
                                break;

                            // Dark Embrace
                            case 3:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(4, 5);
                                card.SetRandomCounter(4, 4);
                                card.SetRandomPassive(WhenCardDestroyedDraw.ID, 1, 1);
                                break;

                            // Feed
                            case 4:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(4, 6);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomPassive(OnKillIncreaseHealthToSelf.ID, 2, 3);
                                break;

                            // Double Tap
                            case 5:
                                card.SetRandomHealth(9, 11);
                                card.SetRandomDamage(4, 5);
                                card.SetRandomCounter(5, 5);
                                card.SetRandomPassive(OnCardPlayedApplyDoubleTapToRandomAttackInHand.ID, 1, 1);
                                break;

                            // Combust
                            case 6:
                                card.SetRandomHealth(10, 14);
                                card.SetRandomDamage(2, 3);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomPassive(OnCardPlayedLoseHealthSelf.ID, 1, 1);
                                card.SetRandomPassive(OnCardPlayedCombustEnemies.ID, 1, 1);
                                break;
                        }
                    };

                    data.createScripts = new CardScript[]
                    {
                        ironcladScript
                    };
                });
        }
    }
}
