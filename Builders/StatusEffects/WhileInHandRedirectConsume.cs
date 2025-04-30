using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhileInHandRedirectConsume : SpirefrostBuilder
    {
        internal static string ID => "While In Hand Redirect Consume";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return new StatusEffectDataBuilder(MainModFile.instance)
                .Create<StatusEffectRedirectConsume>(ID)
                .WithText("While in hand, when a card would <keyword=consume>, <Consume> this instead")
                .WithCanBeBoosted(false);
        }
    }
}
