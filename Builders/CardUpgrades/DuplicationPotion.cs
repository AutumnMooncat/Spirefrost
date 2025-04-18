using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.CardUpgrades
{
    [ToPoolList(PoolListType.Charms)]
    internal class DuplicationPotion : SpirefrostBuilder
    {
        internal static string ID => "DuplicationPotionCharm";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardUpgradeDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/DuplicationCharm.png")
                .WithTitle("Duplication Potion")
                .WithText($"Create a copy of an <Item>") // \nDoes not take up a charm slot
                .WithTier(2)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    CardScriptRunnable duplicationScript = ScriptableObject.CreateInstance<CardScriptRunnable>();
                    duplicationScript.runnable = card =>
                    {
                        if (!MainModFile.instance.looping)
                        {
                            // Safety Lock
                            MainModFile.instance.looping = true;

                            System.Collections.IEnumerator logic = SpirefrostUtils.DuplicationLogic(data, card);

                            while (logic.MoveNext())
                            {
                                object n = logic.Current;
                                Debug.Log("Duplication Script: " + n);
                            }
                        }
                    };
                    data.scripts = new CardScript[]
                    {
                        duplicationScript
                    };
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintIsItem>()
                    };
                    //data.takeSlot = false;
                });
        }
    }
}
