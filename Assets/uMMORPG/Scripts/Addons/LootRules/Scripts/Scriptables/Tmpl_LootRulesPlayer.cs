using UnityEngine;

[CreateAssetMenu(menuName = "ADDON/Templates/Loot Rules player Configurration", order = 998)]
public class Tmpl_LootRulesPlayer : ScriptableObject
{
    [Header("[Enable/Disable Drop system]")]
    public bool enableDropEquipment = false;
    public bool enableDropGold = false;
    public bool enableDropInventory = false;

    [Header("[DROP SETTINGS]")]
    [Tooltip("[Optional] Modifies the drop chance of every item equipped")]
    [Range(-1, 1)] public float equipmentDropModifier;

    [Tooltip("[Optional] Modifies the drop chance of every item in the inventory")]
    [Range(-1, 1)] public float inventoryDropModifier;

    [Header("[ITEM DROP]")]
    public float radiusMultiplier = 1;

#if _iMMOITEMDROP
    [Tooltip("[Required] Drop prefab used for gold drops")]
    public ItemDrop goldDropPrefab;
#endif

    [Tooltip("[Required] Percentage of gold dropped on death")]
    [Range(0, 1)] public float goldPercentage;
}
