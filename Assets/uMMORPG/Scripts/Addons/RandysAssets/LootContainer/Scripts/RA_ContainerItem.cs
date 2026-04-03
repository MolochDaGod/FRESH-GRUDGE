using ConditionalHide;
using UnityEngine;

[System.Serializable]
public class RA_ContainerItem {
    public RA_ContainerItemType ItemType = RA_ContainerItemType.Item;

    [ConditionalHide("ItemType", RA_ContainerItemType.Item)] public ScriptableItem ScriptableItem;
    [ConditionalHide("ItemType", RA_ContainerItemType.Item)] public int StackSizeMin;
    [ConditionalHide("ItemType", RA_ContainerItemType.Item)] public int StackSizeMax;

    [ConditionalHide("ItemType", RA_ContainerItemType.Gold)] public int GoldAmountMin;
    [ConditionalHide("ItemType", RA_ContainerItemType.Gold)] public int GoldAmountMax;

    [ConditionalHide("ItemType", RA_ContainerItemType.Coins)] public int CoinsAmountMin;
    [ConditionalHide("ItemType", RA_ContainerItemType.Coins)] public int CoinsAmountMax;

    [Range(0, 1)] public float Probability;

    [HideInInspector] public int Amount;

    public RA_ContainerItem(ScriptableItem scriptableItem, int stackSizeMin, int stackSizeMax, float probability) {
        ScriptableItem = scriptableItem;
        StackSizeMin = stackSizeMin;
        StackSizeMax = stackSizeMax;
        Probability = probability;
    }

    public RA_ContainerItem(RA_ContainerItemType itemType, ScriptableItem scriptableItem, int stackSize) {
        ItemType = itemType;
        ScriptableItem = scriptableItem;
        Amount = stackSize;
    }

    public RA_ContainerItem(RA_ContainerItemType itemType, int amount) {
        ItemType = itemType;
        Amount = amount;
    }
}