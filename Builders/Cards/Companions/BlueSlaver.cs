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
                .SetStats(6, 6, 5)
                .WithValue(50)
                .SetTraits(TStack("Longshot", 1))
                .WithEyes(FullID, (0.475f, 0.75f, 0.55f, 0.55f, -35f))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Frost", 3)
                    };
                });
        }
    }
}
