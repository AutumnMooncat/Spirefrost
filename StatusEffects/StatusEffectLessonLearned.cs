using System.Linq;

namespace Spirefrost
{
    public class StatusEffectLessonLearned : StatusEffectData
    {
        public override void Init()
        {
            Events.OnBattleWinPreRewards += UpgradeCard;
        }

        public void OnDestroy()
        {
            Events.OnBattleWinPreRewards -= UpgradeCard;
        }

        private void UpgradeCard()
        {
            target.curveAnimator.Ping();
            CardData randomCard = References.PlayerData.inventory.deck.list.Where(data => data.hasAttack && data.IsItem).ToList().RandomItem();
            if (randomCard)
            {
                randomCard.damage += GetAmount();
            }
        }
    }
}
