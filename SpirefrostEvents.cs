﻿using Spirefrost;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

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
    }
}
