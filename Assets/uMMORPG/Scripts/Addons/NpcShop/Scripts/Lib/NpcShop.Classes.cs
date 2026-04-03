using UnityEngine;

// ===================================================================================
// NPC SHOP
// ===================================================================================
[System.Serializable]
public class SellUniqueItems
{
    public ScriptableItem saleUniqueItems;
    [Header("[-=-[ Single item by World/account ]-=-]")]
    public bool isUniqueWorldItem;
}

[System.Serializable]
public struct SoldUniqueItems
{
    public string itemName;
    public string character;
}