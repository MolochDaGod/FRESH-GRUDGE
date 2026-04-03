using UnityEngine;
// HARVEST ITEMS
[System.Serializable]
public class HarvestingHarvestItems
{
    public ScriptableItem template;
    [Range(0, 1)] public float probability;
    [Range(1, 999)] public int minAmount = 1;
    [Range(1, 999)] public int maxAmount = 1;
}