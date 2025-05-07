using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectApplyXWhenRetained : StatusEffectApplyX
    {
        public override void Init()
        {
            SpirefrostEvents.OnCardsRetained += OnRetain;
        }

        public void OnDestroy()
        {
            SpirefrostEvents.OnCardsRetained -= OnRetain;
        }

        private void OnRetain(List<Entity> retained)
        {
            if (retained.Contains(target))
            {
                IEnumerator logic = RunLogic();
                while (logic.MoveNext())
                {
                    object obj = logic.Current;
                }
            }
        }

        private IEnumerator RunLogic()
        {
            Routine.Clump clump = new Routine.Clump();
            clump.Add(Run(GetTargets()));
            yield return clump.WaitForEnd();
        }
    }
}
