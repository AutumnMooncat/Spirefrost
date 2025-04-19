using Deadpan.Enums.Engine.Components.Modding;
using UnityEngine;

namespace Spirefrost.Builders.Tribes
{
    internal class SpireTribe : SpirefrostBuilder
    {
        internal static string ID => "Spire";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return TribeCopy("Basic", ID) //Snowdweller = "Basic", Shademancer = "Magic", Clunkmaster = "Clunk"
                .WithFlag("Images/SpireFlag.png")
                .WithSelectSfxEvent(FMODUnity.RuntimeManager.PathToEventReference("event:/sfx/status/shell")) //event:/sfx/status/heal
                .SubscribeToAfterAllBuildEvent(
                    (data) =>
                    {
                        // Copy the previous prefab
                        GameObject gameObject = data.characterPrefab.gameObject.InstantiateKeepName();

                        // GameObject may be destroyed if their scene is unloaded. This ensures that will never happen to our gameObject
                        Object.DontDestroyOnLoad(gameObject);

                        // For comparison, the snowdweller character is named "Player (Basic)"
                        gameObject.name = "Player (Spirefrost.Spire)";

                        // Set the characterPrefab to our new prefab
                        data.characterPrefab = gameObject.GetComponent<Character>();

                        // Used to track win/loss statistics for the tribe. Not displayed anywhere though :/
                        data.id = "Spirefrost.Spire";

                        // Set title
                        data.characterPrefab.title = gameObject.name;

                        // Setup data
                        data.leaders = DataList<CardData>(MainModFile.PoolToIDs(MainModFile.PoolListType.Leaders));

                        Inventory inventory = ScriptableObject.CreateInstance<Inventory>();
                        inventory.deck.list = DataList<CardData>(MainModFile.PoolToIDs(MainModFile.PoolListType.StarterItems)).ToList();
                        data.startingInventory = inventory;

                        RewardPool unitPool = CreateRewardPool("SpireUnitPool", "Units", DataList<CardData>(
                            MainModFile.PoolToIDs(MainModFile.PoolListType.Units)));

                        RewardPool itemPool = CreateRewardPool("SpireItemPool", "Items", DataList<CardData>(
                            MainModFile.PoolToIDs(MainModFile.PoolListType.Items)));

                        // Dont forget Scrap Charm
                        RewardPool charmPool = CreateRewardPool("SpireCharmPool", "Charms", DataList<CardUpgradeData>(
                            MainModFile.PoolToIDs(MainModFile.PoolListType.Charms).With("CardUpgradeScrap")));

                        data.rewardPools = new RewardPool[]
                        {
                            unitPool,
                            itemPool,
                            charmPool,
                            Extensions.GetRewardPool("GeneralUnitPool"),
                            Extensions.GetRewardPool("GeneralItemPool"),
                            Extensions.GetRewardPool("GeneralCharmPool"),
                            Extensions.GetRewardPool("GeneralModifierPool"),
                            // The snow pools are not Snowdwellers, there are general snow units/cards/charms
                            Extensions.GetRewardPool("SnowUnitPool"),
                            Extensions.GetRewardPool("SnowItemPool"),
                            Extensions.GetRewardPool("SnowCharmPool"),
                        };
                    });
        }
    }
}
