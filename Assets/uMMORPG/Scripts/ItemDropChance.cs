// Defines the drop chance of an item for monster loot generation.
using System;
using UnityEngine;

[Serializable]
public class ItemDropChance
{
    [Range(1, 999)] public int minStack = 1;
    [Range(1, 999)] public int maxStack = 1;
    public ScriptableItem item;
    [Range(0,1)] public float probability;
}
