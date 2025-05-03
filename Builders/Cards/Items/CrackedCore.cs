using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Items
{
    [ToPoolList(PoolListType.DefectStarterItems, 1, 1)]
    internal class CrackedCore : SpirefrostBuilder
    {
        internal static string ID => "crackedcore";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateItem(ID, "Cracked Core")
                .SetSprites("Items/CrackedCore.png", "Items/CrackedCoreBG.png")
                .WithValue(25)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(InstantChannelLightning.ID, 1)
                    };
                });
        }
    }
}
