using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.SilentItems)]
    internal class Tingsha : SpirefrostBuilder
    {
        internal static string ID => "tingsha";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Tingsha")
                .SetSprites("Items/Tingsha.png", "Items/TingshaBG.png")
                .WithValue(60)
                .CanPlayOnHand(true)
                .SetTraits(TStack("Consume", 1))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("Reduce Max Health", 3),
                        SStack(InstantGainBombard.ID, 1),
                        SStack(RunnableResetBombard.ID, 1)
                    };
                });
        }
    }
}
