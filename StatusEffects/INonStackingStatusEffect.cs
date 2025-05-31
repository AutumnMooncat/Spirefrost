using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spirefrost.StatusEffects
{
    internal interface INonStackingStatusEffect
    {
        StatusIcon Icon { get; set; }
    }
}
