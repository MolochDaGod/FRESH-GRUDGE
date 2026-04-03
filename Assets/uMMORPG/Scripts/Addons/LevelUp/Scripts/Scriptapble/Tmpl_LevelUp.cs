using UnityEngine;

// Level Up Template
[CreateAssetMenu(fileName = "Level Up Template", menuName = "ADDON/Templates/Level Up Template", order = 999)]
public partial class Tmpl_LevelUp : ScriptableObject
{
    [Header("[-=-[ LEVEL UP NOTICE ]-=-]")]
    public Tools_PopupClass levelUpNotice = new Tools_PopupClass { message = "Level Up ", suffix = ""};

    [Header("[Optional : Add a Effect to levelup]")]
    [Tooltip("Exemple Add : prefab 'Health Aura' or 'Therapy' (One Time Target Skill Effect)")]
    public OneTimeTargetSkillEffect levelUpEffect = null;

    [Header("[-=-[ Level Up Skill Experience Reward (for each level) ]-=-]")]
    public LinearInt skillExpOnLevelUp;

    [Header("[-=-[ Level Up Reward ]-=-]")]
    public LevelUpReward[] levelUpRewards;

    [Header("[-=-[ Chat Message ]-=-]")]
    public string rewardGoldchatText = "Level {LEVELUP} : you win {AMOUNT} Gold{S}";
#if _iMMOTITLES
    public string rewardTitlechatText = "Level {LEVELUP} : you win new Title {TITLENAME}";
#endif
    public string rewardSkillExpchatText = "Level {LEVELUP} : you win {AMOUNT} Skill Experience";
#if _iMMOHONORSHOP
    public string rewardHonnorCurrencychatText = "Level {LEVELUP} : you win {AMOUNT} {CURRENCYNAME}";
#endif
    public string rewardItemChatText = "Level {LEVELUP} : you win {ITEM} x{AMOUNT}";
}
