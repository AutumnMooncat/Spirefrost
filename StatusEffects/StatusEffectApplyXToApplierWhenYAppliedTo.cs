using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectApplyXToApplierWhenYAppliedTo : StatusEffectApplyX
    {
        public bool instead;

        public bool whenAnyApplied;

        public string[] whenAppliedTypes = new string[1] { "snow" };

        public ApplyToFlags whenAppliedToFlags;

        public bool mustReachAmount;

        public bool adjustAmount;

        public int addAmount;

        public float multiplyAmount = 1f;

        public override void Init()
        {
            base.PostApplyStatus += Run;
        }

        public bool CheckType(StatusEffectData effectData)
        {
            if (effectData.isStatus)
            {
                if (!whenAnyApplied)
                {
                    return whenAppliedTypes.Contains(effectData.type);
                }

                return true;
            }

            return false;
        }

        public override bool RunApplyStatusEvent(StatusEffectApply apply)
        {
            if ((adjustAmount || instead) && target.enabled && !TargetSilenced() && (target.alive || !targetMustBeAlive) && (bool)apply.effectData && apply.count > 0 && CheckType(apply.effectData) && CheckTarget(apply.target))
            {
                if (instead)
                {
                    apply.effectData = effectToApply;
                }

                if (adjustAmount)
                {
                    apply.count += addAmount;
                    apply.count = Mathf.RoundToInt((float)apply.count * multiplyAmount);
                }
            }

            return false;
        }

        public override bool RunPostApplyStatusEvent(StatusEffectApply apply)
        {
            if (target.enabled && !TargetSilenced() && (bool)apply.effectData && apply.count > 0 && CheckType(apply.effectData) && CheckTarget(apply.target))
            {
                return CheckAmount(apply);
            }

            return false;
        }

        public IEnumerator Run(StatusEffectApply apply)
        {
            return Run(new List<Entity>() { apply.applier }, apply.count);
        }

        public bool CheckFlag(ApplyToFlags whenAppliedTo)
        {
            return (whenAppliedToFlags & whenAppliedTo) != 0;
        }

        public bool CheckTarget(Entity entity)
        {
            if (!Battle.IsOnBoard(target))
            {
                return false;
            }

            if (entity == target)
            {
                return CheckFlag(ApplyToFlags.Self);
            }

            if (entity.owner == target.owner)
            {
                return CheckFlag(ApplyToFlags.Allies);
            }

            if (entity.owner != target.owner)
            {
                return CheckFlag(ApplyToFlags.Enemies);
            }

            return false;
        }

        public bool CheckAmount(StatusEffectApply apply)
        {
            if (!mustReachAmount)
            {
                return true;
            }

            StatusEffectData statusEffectData = apply.target.FindStatus(apply.effectData.type);
            if ((bool)statusEffectData)
            {
                return statusEffectData.count >= GetAmount();
            }

            return false;
        }
    }
}
