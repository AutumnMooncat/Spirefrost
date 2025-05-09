using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects;
using Spirefrost.Builders.StatusEffects.IconEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.IroncladUnits)]
    internal class Taskmaster : SpirefrostBuilder
    {
        internal static string ID => "taskmaster";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Taskmaster")
                .SetSprites("Units/Taskmaster.png", "Units/TaskmasterBG.png")
                .SetStats(7, 3, 4)
                .WithValue(50)
                .SetTraits(TStack("Fury", 2))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(TriggerWhenAttackAppliedToSelf.ID, 1)
                    };
                });
        }
    }
}
