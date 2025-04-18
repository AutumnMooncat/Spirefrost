using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using UnityEngine;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Leaders
{
    [ToPoolList(PoolListType.Leaders)]
    internal class Defect : SpirefrostBuilder
    {
        internal static string ID => "defect";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Defect")
                .SetSprites("Leaders/Defect.png", "Leaders/DefectBG.png")
                .SetStats(7, 3, 4)
                .WithValue(25)
                .WithCardType("Leader")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    CardScriptRunnable defectScript = ScriptableObject.CreateInstance<CardScriptRunnable>();
                    defectScript.runnable = card =>
                    {
                        card.GiveUpgrade();
                        int ability = new Vector2Int(0, 6).Random();
                        switch (ability)
                        {
                            // Lightning
                            case 0:
                                card.SetRandomHealth(7, 9);
                                card.SetRandomDamage(0, 1);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomPassive(OnTurnSummonLightningOrb.ID, 1, 1);
                                break;

                            // Dark
                            case 1:
                                card.SetRandomHealth(5, 7);
                                card.SetRandomDamage(0, 1);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomPassive(OnTurnSummonDarkOrb.ID, 1, 1);
                                break;

                            // Plasma
                            case 2:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(5, 7);
                                card.SetRandomCounter(6, 7);
                                card.SetRandomPassive(OnTurnSummonPlasmaOrb.ID, 1, 1);
                                break;

                            // Frost
                            case 3:
                                card.SetRandomHealth(6, 8);
                                card.SetRandomDamage(3, 5);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomPassive(OnTurnSummonFrostOrb.ID, 1, 1);
                                break;

                            // Claw
                            case 4:
                                card.SetRandomHealth(7, 9);
                                card.SetRandomDamage(3, 4);
                                card.SetRandomCounter(4, 4);
                                card.SetRandomPassive(WhenItemPlayedIncreaseItsAttack.ID, 1, 1);
                                break;

                            // Sunder 
                            case 5:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(4, 6);
                                card.SetRandomCounter(5, 5);
                                card.SetRandomPassive(OnKillReduceMaxCounter.ID, 1, 1);
                                break;

                            // Amplify 
                            case 6:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(3, 4);
                                card.SetRandomCounter(4, 4);
                                card.SetRandomPassive(OnCardPlayedApplyAmplifyToRandomItemInHand.ID, 1, 1);
                                break;
                        }
                    };

                    data.createScripts = new CardScript[]
                    {
                        defectScript
                    };
                });
        }
    }
}
