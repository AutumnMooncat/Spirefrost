using System;
using System.Collections;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectWhileActiveXCustom : StatusEffectWhileActiveX
    {
        public override void Init()
        {
            base.Init();
            OnEntityDestroyed += DestroyCheck;
        }

        private IEnumerator DestroyCheck(Entity entity, DeathType deathType)
        {
            yield return Reset();
        }

        public override IEnumerator CardMove(Entity entity)
        {
            if (active && entity != target && (affected.Contains(entity) || affected.Count == 0))
            {
                yield return Reset();
            }
            yield return base.CardMove(entity);
        }

        public override bool RunEntityDestroyedEvent(Entity entity, DeathType deathType)
        {
            return active && entity != target && (affected.Contains(entity) || affected.Count == 0);
        }

        private IEnumerator Reset()
        {
            yield return Deactivate();
            yield return Activate();
        }
    }
}
