using UnityEngine;

// DAILY REWARDS - TEMPLATE
[CreateAssetMenu(fileName = "Tmpl_DailyRewards", menuName = "ADDON/Templates/New DailyRewards", order = 999)]
public class Tmpl_DailyRewards : ScriptableObject
{
    [Header("[-=-=-[ Daily Rewards v2 ]-=-=-]")]
    [Tooltip("One click deactivation")]
    public bool isActive = true;
    public bool autoClaimRewards = false;
    //public bool autoDisplayWindowsRewards = false;

    //[Tooltip("7 day = 1 week , 14 dday = 2 week")]
    //public SplitMonthInWeek splitMonthInWeek = SplitMonthInWeek.Month;

    // [Header("[-=-=-[ Default Day Reward ]-=-=-]")]
    // [Tooltip("If a day have not reward is replaced by this reward")]
    // public DailyReward reward;
    /*
    [Tooltip("Min player level to enable the reward")]
    public int MinLevel = 1;

    [Tooltip("This quest must be completed first")]
#if _iMMOQUESTS
    public Scriptable_Quest requiredQuest;

#else
	public ScriptableQuest requiredQuest;
#endif
    */

    public MonthlyRewards[] montlyRewards;
}

[System.Serializable]
public class MonthlyRewards
{
    public string monthName;

    [Header("[-=-=-[ Default Day Reward ]-=-=-]")]
    [Tooltip("If a day have not reward is replaced by this reward")]
    public DailyReward reward;

    [Header("[-=-=-[ Day Reward ]-=-=-]")]
    [Tooltip("Define your rewards by adding and editing entries")]
    public DailyReward[] rewards;
}
/*
public enum SplitMonthInWeek { 
    Month = 0, 
    TwoWeek = 14,
    Week = 7 
};*/