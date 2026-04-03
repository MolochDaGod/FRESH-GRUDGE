using Mirror;

public partial class Player
{
    // Removes the equipment item when left clicking.
    [Command]
    public void CmdRemoveEquipmentItem(int index)
    {
#if _SERVER
#if _iMMOCURSEDEQUIPMENT
        // validate
        if ((state == "IDLE" || state == "MOVING" || state == "CASTING") && 0 <= index && index < equipment.slots.Count && equipment.slots[index].amount > 0)
        {
            // check inventory for free slot and pass it to swapinventoryequip()
            ItemSlot item = equipment.slots[index];

            if (inventory.SlotsFree() >= item.amount && ((EquipmentItem)item.item.data).CanUnequipClick(this, (EquipmentItem)item.item.data))
            {
                if (item.amount > 0)
                {
                    inventory.Add(item.item, 1); //TODO faudrait remplacé 1 par item.amount pour les fleches par exemple
                    item.DecreaseAmount(1);
                    equipment.slots[index] = item;
                }
            }
        }

#else
        // validate
        if ((state == "IDLE" || state == "MOVING" || state == "CASTING") && 0 <= index && index < equipment.slots.Count && equipment.slots[index].amount > 0)
        {
            // check inventory for free slot and pass it to swapinventoryequip()
            ItemSlot item = equipment.slots[index];

            if (inventory.SlotsFree() >= item.amount)
            {
                if (item.amount > 0)
                {
                    inventory.Add(item.item, 1); //TODO faudrait remplacé 1 par item.amount pour les fleches par exemple
                    item.DecreaseAmount(1);
                    equipment.slots[index] = item;
                }
            }
        }

#endif
#endif
    }
}