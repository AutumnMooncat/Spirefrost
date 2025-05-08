using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects.IconEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.Units)]
    internal class BlueSlaver : SpirefrostBuilder
    {
        internal static string ID => "blueslaver";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Slaver")
                .SetSprites("Units/SlaverBlue.png", "Units/SlaverBlueBG.png")
                .SetStats(6, 2, 4)
                .WithValue(50)
                .SetTraits(TStack("Longshot", 1), TStack("Pull", 2))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(Weak.ID, 1)
                    };
                });
        }
    }
}
