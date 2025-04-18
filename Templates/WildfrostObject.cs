using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Templates
{
    internal class WildfrostObject : SpirefrostBuilder
    {
        internal static string ID => null;

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return null;
        }
    }
}
