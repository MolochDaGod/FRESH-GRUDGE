using UnityEngine;

// ===================================================================================
// SIMPLE DAILY REWARD
// ===================================================================================
[System.Serializable]
public class DailyReward
{
    public string id;
    [Range(0,31)] public int dayNumber = 0;

    public Sprite spriteNotClaimed;
    public Sprite spriteClaimed;

    [Header("[-=-=-[ Daily Reward ]-=-=-]")]
    public long rewardGold;

    public long rewardCoins;
    public long rewardExperience;
    public long rewardSkillExperience;

    public Tools_ItemRequirement[] rewardItems;

#if _iMMOHONORSHOP
    public HonorShopCurrencyDrop[] honorCurrencies;
#endif
}