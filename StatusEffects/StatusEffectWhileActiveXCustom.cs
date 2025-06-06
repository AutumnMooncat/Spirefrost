using System.Collections;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectWhileActiveXCustom : StatusEffectWhileActiveX
    {
        public override void Init()
        {
            base.Init();
        }

        public override IEnumerator CardMove(Entity entity)
        {
            if (active && entity != target && affected.Contains(entity) || affected.Count == 0)
            {
                yield return Reset();
            }
            yield return base.CardMove(entity);
        }

        private IEnumerator Reset()
        {
            yield return Deactivate();
            yield return Activate();
        }
    }
}
