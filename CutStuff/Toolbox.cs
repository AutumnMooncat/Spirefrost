using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.Items)]
    internal class Toolbox : SpirefrostBuilder
    {
        internal static string ID => "toolbox";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Toolbox")
                .SetSprites("Items/Toolbox.png", "Items/ToolboxBG.png")
                .WithValue(50)
                .SetTraits(TStack("Consume", 1), TStack("Zoomlin", 1))
                .CanPlayOnHand(true)
                .NeedsTarget(false)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(OnCardPlayedDoDiscoveryToolbox.ID, 3)
                    };
                });
        }
    }
}
