using UnityEngine;

[CreateAssetMenu(fileName = "Warehouse configuration", menuName = "ADDON/Templates/Warehouse Configuration", order = 999)]
public class Tmpl_WarehouseConfiguration : ScriptableObject
{
    [Header("[-=-[ GameEvent ]-=-]")]
    public GameEvent uiEventPlayerWarehouse;

    [Header("[-=-[ Force use Npc ]-=-]")]
    public bool forceUseNpc = false;

    [Header("[-=-[ Storage ]-=-]")]
    public LinearInt warehouseStorageItems = new LinearInt() { baseValue = 10, bonusPerLevel = 10 };
    public LinearLong warehouseStorageGold = new LinearLong() { baseValue = 100000, bonusPerLevel = 100000 };

    [Header("[-=-[ Cost Upgrade ]-=-]")]
    public Tools_Cost[] warehouseUpgradeCost;

    [Header("[-=-[ Allowances ]-=-]")]
    public bool storeSellable = true;
    public bool storeTradable = true;
    public bool storeDestroyable = true;

    [Header("[-=-[ Messages ]-=-]")]
    public string upgradeLabel = "player Warehouse upgraded!";
    public string distanceNpcTooGreat = "The distance between you and the npc is too great for use Warehouse";
}