using UnityEngine;

// GUILD UPGRADES CONFIG
[CreateAssetMenu(fileName = "Guild Upgrades", menuName = "ADDON/Templates/New Guild Upgrades", order = 999)]
public class Tmpl_GuildUpgrades : ScriptableObject
{
    [Header("[CAPACITY]")]
    public LinearInt guildCapacity = new LinearInt { baseValue = 10 };

    [Header("[REWARDS (1 item per level)]")]
    [Tooltip("The player who is upgrading gains the item")]
    public ScriptableItemAndAmount[] rewardItem;

    [Header("[UPRADING]")]
    public Tools_Cost[] guildUpgradeCost;

    [Header("[MESSAGES]")]
    public string guildUpgradeLabel = "Guild upgraded!";
}