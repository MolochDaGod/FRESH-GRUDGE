using UnityEngine;

// ===================================================================================
// ITEM DROP - SCRIPTABLE ITEM
// ===================================================================================
public partial class ScriptableItem
{
    [Header("[-=-[ITEM DROP]-=-]")]
    [Tooltip("[Required] Players only: Chance to drop the item from inventory to the ground when dead.")]
    [Range(0, 1)] public float dropChance = 0f;

    [Tooltip("[Required] Drop prefab that is used to represent the item, requires a ItemDrop component on it.")]
    public ItemDrop dropPrefab;

    [Header("Auto PickUp")]
    public bool autoPickUp;
    // -----------------------------------------------------------------------------------
}
