/*using Mirror;

// PLAYER
public partial class PlayerEquipment
{

    // -----------------------------------------------------------------------------------
    // CmdSwapEquipInventory
    // -----------------------------------------------------------------------------------
    [Command]
    public void CmdSwapEquipInventory(int equipmentIndex, int inventoryIndex)
    {
#if _SERVER
        SwapEquipInventory(equipmentIndex, inventoryIndex);
#endif
    }

    // -----------------------------------------------------------------------------------
    // OnDragAndDrop_EquipmentSlot_InventorySlot
    // -----------------------------------------------------------------------------------
    void OnDragAndDrop_EquipmentSlot_InventorySlot(int[] slotIndices)
    {
        // merge? check Equals because name AND dynamic variables matter (petLevel etc.)
        // => merge is important when dragging more arrows into an arrow slot!
        if (slots[slotIndices[0]].amount > 0 && inventory.slots[slotIndices[1]].amount > 0 && slots[slotIndices[0]].item.Equals(inventory.slots[slotIndices[1]].item))
        {
            CmdMergeEquipInventory(slotIndices[0], slotIndices[1]);
        }
        // swap?
        else
        {
            CmdSwapEquipInventory(slotIndices[0], slotIndices[1]); // reversed
        }
    }
#if _SERVER
    [Server]
    public void SwapEquipInventory(int equipmentIndex, int inventoryIndex)
    {
        // validate: make sure that the slots actually exist in the inventory
        // and in the equipment
        if (inventory.InventoryOperationsAllowed() && 0 <= inventoryIndex && inventoryIndex < inventory.slots.Count && 0 <= equipmentIndex && equipmentIndex < slots.Count)
        {
            // equipment slot has to be empty (unequip) or un-equipable
            ItemSlot slot = slots[equipmentIndex];
            
            if (slot.amount == 0 || slot.item.data is EquipmentItem itemData && itemData.CanUnequip(player, inventoryIndex, equipmentIndex))
            {
                // swap them
                ItemSlot temp = inventory.slots[inventoryIndex];
                inventory.slots[inventoryIndex] = slot;
                slots[equipmentIndex] = temp;

            }
        }
    }
#endif 
    // -----------------------------------------------------------------------------------
}*/