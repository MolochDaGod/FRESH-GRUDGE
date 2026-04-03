using UnityEngine;

// QUEST REWARD - CLASS
[System.Serializable]
public partial struct QuestReward
{
    [Header("[-=-[ Requirements ]-=-]")]
    [Tooltip("Ignore if the quest has only one reward. When there are more, one is chosen randomly. Does not work with class based rewards.")]
    [Range(0, 1)] public float rewardChance;

    [Tooltip("Reward only available to the added classes, or all if left empty. Overrides random reward Chance! [Assign a player prefab here]")]
    public Player[] availableToClass;

    [Header("[-=-[ Rewards ]-=-]")]
    public long rewardGold;

    public long rewardCoins;
    public long rewardExperience;
    public RewardItem[] rewardItem;
#if _iMMOTRAVEL
    public Unlockroute[] rewardUnlockroutes;
#endif
#if _iMMOHONORSHOP
    public HonorShopCurrencyCost[] honorCurrency;
#endif
#if _iMMOTITLES
    public Tmpl_Titles title;
#endif
#if _iMMOPVP
    public Tmpl_Realm changeRealm;
    public Tmpl_Realm changeAlliedRealm;
#endif
}

//

[System.Serializable]
public partial class PvpDescription
{
    public string pvpKill = "Kill ";
    public string pvpPlayer = " Player(s) ";
    public string pvpLevel = " level ";
    public string pvpParty = "Party.";
    public string pvpGuild = "Guild.";
    public string pvpRealm = "Realm.";
    public string pvpNot = " Not ";
    public string pvpMy = "in my ";
}

// PVP TARGET - CLASS
[System.Serializable]
public partial class PvpTarget
{
    public int levelRange;
    public int amount;

    [Tooltip("AnyGuild = Target must be in a guild!\nAnyParty = Target must be in a party!\nMyParty/OtherParty = Both must be in a party!\nMyGuild/OtherGuild = Both must be in a Guild!")]
    public pvpType type;

    public enum pvpType { Any, AnyGuild, MyGuild, OtherGuild, AnyParty, MyParty, OtherParty, MyRealm, OtherRealm }
}

// KILL TARGET - CLASS
[System.Serializable]
public partial class KillTarget
{
    public Monster target;
    public int amount;
}

// GATHER TARGET - CLASS
[System.Serializable]
public partial class GatherTarget
{
    public ScriptableItem target;
    public int amount;
}

// HARVEST TARGET - CLASS
#if _iMMOHARVESTING
[System.Serializable]
public partial class HarvestTarget
{
    public HarvestingProfessionTemplate target;
    public int amount;
}
#endif

// CRAFT TARGET - CLASS
#if _iMMOCRAFTING
[System.Serializable]
public partial class CraftTarget
{
    public Tmpl_Recipe target;
    // public Tmpl_CraftingRecipe target;
    public int amount;
}
#endif

// LOOT TARGET - CLASS
#if _iMMOCHEST
[System.Serializable]
public partial class LootTarget
{
    public Lootcrate target;
    public int amount;
}
#endif

// REWARD ITEM - CLASS
[System.Serializable]
public partial class RewardItem
{
    public ScriptableItem item;
    public int amount;
}