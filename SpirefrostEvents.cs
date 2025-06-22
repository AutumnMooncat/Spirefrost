using Dead;
using Spirefrost;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using static Events;

namespace Spirefrost
{
    internal class SpirefrostEvents
    {
        internal static event UnityAction<Entity> OnMovedByDiscarder;

        internal static void InvokeMovedByDiscarder(Entity entity)
        {
            UnityAction<Entity> onMovedByDiscarder = OnMovedByDiscarder;
            if (onMovedByDiscarder == null)
            {
                return;
            }
            onMovedByDiscarder(entity);
        }

        internal static event UnityAction<List<Entity>> OnCardsRetained;

        internal static void InvokeCardsRetained(List<Entity> entities)
        {
            UnityAction<List<Entity>> onCardsRetained = OnCardsRetained;
            if (onCardsRetained == null)
            {
                return;
            }
            onCardsRetained(entities);
        }

        internal delegate IEnumerator CounterResetHandler(Entity entity);

        internal static event CounterResetHandler OnCounterReset;

        internal static IEnumerator CounterResetRoutine(Entity entity)
        {
            return OnCounterReset(entity);
        }

        internal static bool HasCounterReset()
        {
            return OnCounterReset != null;
        }

        internal delegate void StatusReductionEvent(StatusEffectData status, ref int amount, bool temporary);

        internal static event StatusReductionEvent OnPreStatusReduction;

        internal static void InvokePreStatusReduction(StatusEffectData status, ref int amount, bool temporary)
        {
            StatusReductionEvent onPreStatusReduction = OnPreStatusReduction;
            if (onPreStatusReduction == null)
            {
                return;
            }
            onPreStatusReduction(status, ref amount, temporary);
        }

        internal static event UnityActionRef<Trigger, bool> OnIgnoreTriggerCheck;

        internal static void InvokeIgnoreTriggerCheck(ref Trigger trigger, ref bool ignore)
        {
            UnityActionRef<Trigger, bool> onIgnoreTriggerCheck = OnIgnoreTriggerCheck;
            if (onIgnoreTriggerCheck == null)
            {
                return;
            }
            onIgnoreTriggerCheck(ref trigger, ref ignore);
        }
    }
}
