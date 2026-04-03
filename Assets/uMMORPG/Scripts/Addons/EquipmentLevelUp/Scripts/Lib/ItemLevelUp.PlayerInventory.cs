
using Mirror;
using UnityEngine;

public partial class PlayerInventory
{
    public GameEvent EquipmentlevelUpEvent;
    //private int slotIndex = -1;
    private Item itemInLevelUpSlot;

    [TargetRpc]
    public void OnInventoryChanged_ItemLevelUp()
    {
        EquipmentlevelUpEvent.TriggerEventInt(-1);
        itemInLevelUpSlot = default;
    }

    void OnDragAndDrop_InventorySlot_EquipmentLevelUpSlot(int[] slotIndices)
    {
        if (slots[slotIndices[0]].item.data is EquipmentItem)
        {
            EquipmentlevelUpEvent.TriggerEventInt(slotIndices[0]);
            itemInLevelUpSlot = slots[slotIndices[0]].item;
        }
    }

    void OnDragAndClear_EquipmentLevelUpSlot(int slotIndex)
    {
        EquipmentlevelUpEvent.TriggerEventInt(-1);
        itemInLevelUpSlot = default;
        slotIndex = default;
    }

    // by Arkax 
    public void CheckResetEquipmentLevelUp(Item item)
    {
        if (item.Equals(itemInLevelUpSlot))
        {
            EquipmentlevelUpEvent.TriggerEventInt(-1);
            itemInLevelUpSlot = default;
        }
    }
}
