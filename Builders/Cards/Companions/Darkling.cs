using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects.IconEffects;
using static Spirefrost.MainModFile;
using static Spirefrost.SpirefrostUtils.AutoAdd;

namespace Spirefrost.Builders.Cards.Companions
{
    [ToPoolList(PoolListType.IroncladUnits)]
    internal class Darkling : SpirefrostBuilder
    {
        internal static string ID => "darkling";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Darkling")
                .SetSprites("Units/Darkling.png", "Units/DarklingBG.png")
                .SetStats(4, 2, 5)
                .WithValue(50)
                .WithEyes(FullID, 
                (0.475f, 0.525f, 1.1f, 1.1f, 0f),
                (1.075f, 0.575f, 1f, 1f, 0f),
                (-0.45f, 1.475f, 0.7f, 0.7f, 20f),
                (-0.2f, 1.6f, 0.7f, 0.7f, 20f))
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(Regrow.ID, 2),
                        SStack("When Card Destroyed, Gain Attack & Health", 1)
                    };
                });
        }
    }
}
