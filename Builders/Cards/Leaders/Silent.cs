using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using Spirefrost.Builders.StatusEffects.IconEffects;
using UnityEngine;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Leaders
{
    [ToPoolList(PoolListType.Leaders)]
    internal class Silent : SpirefrostBuilder
    {
        internal static string ID => "silent";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Silent")
                .SetSprites("Leaders/Silent.png", "Leaders/SilentBG.png")
                .SetStats(6, 2, 3)
                .WithValue(25)
                .WithCardType("Leader")
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    CardScriptRunnable silentScript = ScriptableObject.CreateInstance<CardScriptRunnable>();
                    silentScript.runnable = card =>
                    {
                        card.GiveUpgrade();
                        int ability = new Vector2Int(0, 6).Random();
                        switch (ability)
                        {
                            // Accuracy
                            case 0:
                                card.SetRandomHealth(7, 9);
                                card.SetRandomDamage(3, 4);
                                card.SetRandomCounter(4, 4);
                                card.SetRandomPassive("While Active Increase Attack To Items In Hand", 1, 1);
                                break;

                            // Envenom
                            case 1:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(2, 2);
                                card.SetRandomCounter(4, 4);
                                card.SetRandomPassive(WhenEnemyIsHitByItemApplyShroomToThem.ID, 1, 1);
                                break;

                            // Grand Finale - Removed
                            /*case 2:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(3, 4);
                                card.SetRandomCounter(3, 4);
                                card.SetRandomPassive("On Hit Damage If Draw Pile Empty", 4, 4);
                                break;*/

                            // Blade Dance
                            case 2:
                                card.SetRandomHealth(8, 11);
                                card.SetRandomDamage(1, 2);
                                card.SetRandomCounter(3, 4);
                                card.SetRandomPassive(OnCardPlayedAddShivToHand.ID, 2, 2);
                                break;

                            // Malaise
                            case 3:
                                card.SetRandomHealth(6, 9);
                                card.SetRandomDamage(3, 5);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomActive(Weak.ID, 1, 1);
                                break;

                            // Caltrops
                            case 4:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(2, 3);
                                card.SetRandomCounter(3, 4);
                                card.SetRandomPassive("Teeth", 2, 3);
                                break;

                            // Flechettes
                            case 5:
                                card.SetRandomHealth(7, 9);
                                card.SetRandomDamage(2, 3);
                                card.SetRandomCounter(3, 4);
                                card.SetRandomPassive(BonusDamageEqualToSkillsInHand.ID, 1, 1);
                                break;

                            // Alchemize
                            case 6:
                                card.SetRandomHealth(8, 10);
                                card.SetRandomDamage(3, 5);
                                card.SetRandomCounter(4, 5);
                                card.SetRandomPassive(OnCardPlayedGainRandomCharm.ID, 1, 1);
                                break;
                        }
                    };

                    data.createScripts = new CardScript[]
                    {
                        silentScript
                    };
                });
        }
    }
}
