using UnityEngine;

// Class representing a skin and its associated Steam Product ID
[System.Serializable]
public class ItemSkinWithID
{
    public ScriptableItem skin; // Reference to the ScriptableItem for the skin
    public string steamProductId; // Steam Product ID for the skin
}


public partial class ScriptableItem
{
    [Header("Item Skins")]
    public ItemSkinWithID[] itemSkins; // Array that holds skins and their Steam Product IDs
}