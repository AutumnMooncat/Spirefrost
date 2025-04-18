using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spirefrost
{
    internal class SpirefrostBuilder
    {
        internal static T TryGet<T>(string name) where T : DataFile
        {
            return MainModFile.instance.TryGet<T>(name);
        }

        internal static CardData.StatusEffectStacks SStack(string name, int amount) => new CardData.StatusEffectStacks(TryGet<StatusEffectData>(name), amount);

        internal static CardData.TraitStacks TStack(string name, int amount) => new CardData.TraitStacks(TryGet<TraitData>(name), amount);
    }
}
