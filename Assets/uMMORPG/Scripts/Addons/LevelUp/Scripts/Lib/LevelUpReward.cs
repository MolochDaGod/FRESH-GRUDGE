using System;
using UnityEngine;

// Level Up Reward
[System.Serializable]
public partial class LevelUpReward
{
    [Header("[-=-[ player level ]-=-]")]
    [Min(1)] public int playerLevelUp;
   
    [Header("[-=-[ Reward Gold]-=-]")]
    [Min(0)] public int gold;

    [Tooltip("[Optional] This allow skill Exp reward to specific level")]
    [Header("[-=-[ Reward Skill experience ]-=-]")]
    [Min(0)] public int skillExpReward;

#if _iMMOHONORSHOP
    [Tooltip("[Optional] if you want player reward honors currency on level up")]
    [Header("[-=-[ Reward honors currency ]-=-]")]
    public RewardHonorCurrency[] honors;
#endif

#if _iMMOTITLES
    [Tooltip("[Optional] if you want player earn title in level up")]
    [Header("[-=-[ Reward Earn Title ]-=-]")]
    public Tmpl_Titles[] eanTitles;
#endif

    [Tooltip("[Optional] if you want player reward an item in level up")]
    [Header("[-=-[ Reward item ]-=-]")]
    public RewardItem[] item;
}

#if _iMMOHONORSHOP
[System.Serializable]
public partial class RewardHonorCurrency
{
    public Tmpl_HonorCurrency honors;
    [Min(0)] public int amount;
}
#endif