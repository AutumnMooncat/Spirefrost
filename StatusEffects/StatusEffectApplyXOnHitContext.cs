using UnityEngine;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectApplyXOnHitContext : StatusEffectApplyXOnHit
    {
        public override bool RunPreAttackEvent(Hit hit)
        {
            if (hit.attacker == target && target.alive && target.enabled && (bool)hit.target)
            {
                if (addDamageFactor != 0 || multiplyDamageFactor != 1f)
                {
                    bool flag = true;
                    TargetConstraint[] array = applyConstraints;
                    foreach (TargetConstraint targetConstraint in array)
                    {
                        if (!targetConstraint.Check(hit.target) && (!(targetConstraint is TargetConstraintHasStatus targetConstraintHasStatus) || !targetConstraintHasStatus.CheckWillApply(hit)))
                        {
                            flag = false;
                            break;
                        }
                    }

                    if (flag)
                    {
                        int amount = contextEqualAmount?.Get(hit.target) ?? 0;
                        if (addDamageFactor != 0)
                        {
                            hit.damage += amount * addDamageFactor;
                        }

                        if (multiplyDamageFactor != 1f)
                        {
                            hit.damage = Mathf.RoundToInt((float)hit.damage * multiplyDamageFactor);
                        }
                    }
                }

                if (!hit.Offensive && (hit.damage > 0 || ((bool)effectToApply && effectToApply.offensive)))
                {
                    hit.FlagAsOffensive();
                }

                storedHit.Add(hit);
            }

            return false;
        }
    }
}
