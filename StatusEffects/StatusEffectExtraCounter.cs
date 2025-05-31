using UnityEngine;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectExtraCounter : StatusEffectData, INonStackingStatusEffect
    {
        internal int maxCount;

        public StatusIcon Icon { get => _icon; set => _icon = value; }

        private StatusIcon _icon;

        public override void Init()
        {
            Events.OnEntityPostHit += CheckCounter;
            maxCount = count;
            type = "counter";
            iconGroupName = "counter";
            visible = true;
        }

        public void OnDestroy()
        {
            Events.OnEntityPostHit -= CheckCounter;
        }

        // Called via Battle patch
        internal void ResetCounter()
        {
            count = maxCount;
            target.PromptUpdate();
        }

        private void CheckCounter(Hit hit)
        {
            if (hit.target == target && target.alive && hit.counterReduction != 0 && !hit.dodged && !hit.nullified)
            {
                Debug.Log($"ExtraCounter got hit {hit}, reduction: {hit.counterReduction}");
                count -= hit.counterReduction;
                if (count < 0)
                {
                    count = 0;
                }
                Debug.Log($"New counter: {count}/{maxCount}");
                target.PromptUpdate();

                // Actual trigger handled by Battle patch
            }
        }

        public override object GetMidBattleData()
        {
            return maxCount;
        }

        public override void RestoreMidBattleData(object data)
        {
            if (data is int max)
            {
                maxCount = max;
            }
        }

        public void ModifyMaxCounter(int change, bool set = false)
        {
            if (set)
            {
                maxCount = change;
                count = change;
            }
            else
            {
                maxCount = Mathf.Max(1, maxCount + change);
                count = Mathf.Max(0, count + change);
            }
            Debug.Log($"ExtraCounter Max changed: {count}/{maxCount}");
        }
    }
}
