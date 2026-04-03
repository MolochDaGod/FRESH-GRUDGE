using UnityEngine;

[CreateAssetMenu(menuName = "ADDON/Templates/Item Drop Configurration", order = 998)]
public class Tmpl_ItemDropConfiguration : ScriptableObject
{
    [Header("[-=-[ Item Drop ]-=-]")]
    public bool isActive = true;

    [Header("[-=-[ Item Drop Configuration ]-=-]")]
    [Tooltip("Item Auto Pickup Override")]
    public bool overrideItemAutoPickup = true;
    public bool itemAutoPickup = true;
    [Tooltip("maximum distance around monster from item drop on ground")]
    [Range(1, 5)] public int rangeDropItem = 2;
    public ItemDrop defaultPrefabItemDrop;

    [Header("[-=-[ Gold Item Drop Configuration ]-=-]")]
    [Tooltip("Gold Item Drop Configuration")]
    public bool autoPickupGold = false;
    public ItemDrop prefabGoldItemDrop;

    [Header("[-=-[ player Item Drop ]-=-]")]
    public bool enablePlayerItemDrop = true;

    public bool enableDropOnGround = true;
    [Range(1, 5)] public int playerItemDropRadius = 3;
    [Min(0)] public int maxLevelDifference;
    public EntityType killerEntityForDrop;
    public bool enableDropInventory;
    [Range(1, 100)] public int percentInventoryItemDrop = 20;
    public bool enableDropEquipment;
    [Range(1, 100)] public int percentEquipmentItemDrop = 20;
    public bool enableDropGold;
    [Range(1, 100)] public int percentGoldDrop = 10;
}
