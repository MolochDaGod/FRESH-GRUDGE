using UnityEngine;

[CreateAssetMenu(fileName = "Guild Warehouse", menuName = "ADDON/Templates/Guild Warehouse", order = 999)]
public class Tmpl_GuildWarehouseConfiguration : ScriptableObject
{
    [Header("[-=-[ GameEvent ]-=-]")]
    public GameEvent uiEventGuildWarehouse;

    [Header("[-=-[ Force use Npc ]-=-]")]
    public bool forceUseNpc = true;

    [Header("[-=-[ Storage ]-=-]")]
    public LinearInt guildWarehouseStorageItems = new LinearInt { baseValue = 10 };

    public LinearLong guildWarehouseStorageGold = new LinearLong { baseValue = 10000 };

    [Header("[-=-[ Cost Upgrade ]-=-]")]
    public Tools_Cost[] guildWarehouseUpgradeCost;

    [Header("[-=-[ Allowances ]-=-]")]
    public bool storeSellable = true;
    public bool storeTradable = true;
    public bool storeDestroyable = true;

    [Header("[-=-[ Messages ]-=-]")]
    public string guildWarehouseUpgradeLabel = "Guild Warehouse upgraded!";
}
