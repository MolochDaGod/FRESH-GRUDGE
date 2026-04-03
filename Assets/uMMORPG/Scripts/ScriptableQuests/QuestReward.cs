using System.Collections.Generic;

namespace GrudgeWarlords.Crafting
{
    public class QuestReward
    {
        public int rewardGold;
        public int rewardExperience;
        public int rewardCoins;
        public RewardItem[] rewardItem;
        public float rewardChance;
        public string[] availableToClass;

        public QuestReward(int rewardGold, int rewardExperience, int rewardCoins, RewardItem[] rewardItem, float rewardChance, string[] availableToClass)
        {
            this.rewardGold = rewardGold;
            this.rewardExperience = rewardExperience;
            this.rewardCoins = rewardCoins;
            this.rewardItem = rewardItem;
            this.rewardChance = rewardChance;
            this.availableToClass = availableToClass;
        }
    }
}
