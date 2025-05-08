using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Templates
{
    internal class WildfrostStatusCopy : SpirefrostBuilder
    {
        internal static string ID => null;

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("", ID)
                .WithText("")
                .SubscribeToAfterAllBuildEvent<StatusEffectData>(data =>
                {
                    
                });
        }
    }
}
