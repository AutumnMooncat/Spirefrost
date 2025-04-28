using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects.IconEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.Items)]
    internal class Medkit : SpirefrostBuilder
    {
        internal static string ID => "medkit";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Medical Kit")
                .SetSprites("Items/MedicalKit.png", "Items/MedicalKitBG.png")
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(Regen.ID, 3)
                    };
                });
        }
    }
}
