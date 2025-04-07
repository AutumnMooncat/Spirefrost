using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlayTheFrost
{
    public class CardScriptRunnable : CardScript
    {
        public delegate void ToRun(CardData data);

        public ToRun runnable;
        public override void Run(CardData target)
        {
            runnable(target);
        }
    }

}
