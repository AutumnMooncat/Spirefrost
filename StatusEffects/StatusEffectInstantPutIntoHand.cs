using Spirefrost.Patches;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectInstantPutIntoHand : StatusEffectInstant
    {
        public bool allowItems;

        public bool allowCompanions;

        public bool allowClunkers;

        public override IEnumerator Process()
        {
            List<Entity> validEntities = new List<Entity>();
            foreach (var item in References.Player.drawContainer)
            {
                if (allowItems && item.data.IsItem)
                {
                    validEntities.Add(item);
                }
                else if (allowClunkers && item.data.IsClunker)
                {
                    validEntities.Add(item);
                }
                else if (allowCompanions && item.data.cardType.name == "Friendly")
                {
                    validEntities.Add(item);
                }
            }
            for (int i = 0; i < GetAmount(); i++)
            {
                if (validEntities.Count == 0)
                {
                    if (NoTargetTextSystem.Exists())
                    {
                        if (allowItems && !allowCompanions && !allowClunkers)
                        {
                            NoTargetSystemPatch.noTargetType = NoTargetSystemPatch.NoTargetTypeExtra.NoItemsToMove;
                        }
                        else if (!allowItems && allowCompanions && !allowClunkers)
                        {
                            NoTargetSystemPatch.noTargetType = NoTargetSystemPatch.NoTargetTypeExtra.NoCompanionsToMove;
                        }
                        else if (!allowItems && !allowCompanions && allowClunkers)
                        {
                            NoTargetSystemPatch.noTargetType = NoTargetSystemPatch.NoTargetTypeExtra.NoClunkersToMove;
                        }
                        else
                        {
                            NoTargetSystemPatch.noTargetType = NoTargetSystemPatch.NoTargetTypeExtra.NoCardsToMove;
                        } 
                        yield return NoTargetTextSystem.Run(target, NoTargetType.None);
                    }
                    break;
                }
                Entity toMove = validEntities.RandomItem();
                validEntities.Remove(toMove);
                yield return Sequences.CardMove(toMove, new CardContainer[] { References.Player.handContainer });
                toMove.flipper.FlipUp();
                //yield return Sequences.WaitForAnimationEnd(_selected);
                yield return new ActionRunEnableEvent(toMove).Run();
                toMove.display.hover.enabled = true;
            }
            References.Player.handContainer.TweenChildPositions();
            yield return base.Process();
        }
    }
}
