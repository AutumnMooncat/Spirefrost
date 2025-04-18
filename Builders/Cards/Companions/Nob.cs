using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.StatusEffects.IconEffects;

namespace Spirefrost.Builders.Cards.Companions
{
    internal class Nob : SpirefrostBuilder
    {
        internal static string ID => "nob";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new CardDataBuilder(MainModFile.instance)
                .CreateUnit(ID, "Nob")
                .SetSprites("Units/Nob.png", "Units/NobBG.png")
                .SetStats(12, 3, 5)
                .WithValue(50)
                .SubscribeToAfterAllBuildEvent(data =>
                {
                    MainModFile.instance.maskedSpries.Add(data.name, MainModFile.instance.ImagePath("Units/NobMask.png").ToSprite());
                    data.attackEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack(Vulnerable.ID, 2)
                    };
                    data.startWithEffects = new CardData.StatusEffectStacks[]
                    {
                        SStack("When Hit Increase Attack Effects To Self", 1)
                    };
                });
        }
    }
}
