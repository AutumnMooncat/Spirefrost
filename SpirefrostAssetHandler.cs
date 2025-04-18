using System.Collections.Generic;

namespace Spirefrost
{
    internal class SpirefrostAssetHandler
    {
        public static List<object> assets = new List<object>();

        internal static void CreateAssets()
        {
            assets.AddRange(new SpirefrostUtils.AutoAdd().Process(typeof(SpirefrostBuilder), "GetBuilder", "ID"));
        }
    }
}
