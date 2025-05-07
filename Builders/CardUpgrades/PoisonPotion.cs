using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.CardUpgrades
{
    [ToPoolList(PoolListType.SilentCharms)]
    internal class PoisonPotion : SpirefrostBuilder
    {
        internal static string ID => "PoisonPotionCharm";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardUpgradeDataBuilder(MainModFile.instance)
                .Create(ID)
                .WithType(CardUpgradeData.Type.Charm)
                .WithImage("Charms/PoisonCharm.png")
                .WithTitle("Poison Potion")
                .WithText($"Replace current <keyword=attack> with apply <keyword=shroom>")
                .WithTier(2)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.targetConstraints = new TargetConstraint[]
                    {
                        MakeConstraint<TargetConstraintAttackMoreThan>(t => t.value = 0),
                    };

                    CardScriptReplaceAttackWithApply poisonScript = ScriptableObject.CreateInstance<CardScriptReplaceAttackWithApply>();
                    poisonScript.effect = TryGet<StatusEffectData>("Shroom");
                    data.scripts = new CardScript[]
                    {
                        poisonScript
                    };
                });
        }
    }
}
