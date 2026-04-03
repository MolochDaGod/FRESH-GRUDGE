using Mirror;

public partial class Player
{
    [Command]
    public void CmdSortInventory(SortOption sortOption )
    {
#if _SERVER
        ItemSlot tempItem;

        // Sort the items
        for (int i = 0; i < inventory.slots.Count - 1; i++)
        {
            for (int j = i + 1; j < inventory.slots.Count; j++)
            {
                if (inventory.slots[i].amount == 0 && inventory.slots[j].amount != 0)
                {
                    tempItem = inventory.slots[i];
                    inventory.slots[i] = inventory.slots[j];
                    inventory.slots[j] = tempItem;
                }
                else if (inventory.slots[i].amount != 0 && inventory.slots[j].amount != 0 &&
                         ShouldSwap(inventory.slots[i], inventory.slots[j], sortOption))
                {
                    tempItem = inventory.slots[i];
                    inventory.slots[i] = inventory.slots[j];
                    inventory.slots[j] = tempItem;
                }
            }
        }

        // Move all sorted items to the beginning of the list
        int index = 0;
        foreach (ItemSlot slot in inventory.slots)
        {
            if (slot.amount != 0)
            {
                tempItem = inventory.slots[index];
                inventory.slots[index] = slot;
                inventory.slots[inventory.slots.IndexOf(slot)] = tempItem;
                index++;
            }
        }
#endif
    }
#if _SERVER
    private bool ShouldSwap(ItemSlot slot1, ItemSlot slot2, SortOption sortOption)
    {
        if (//sortOption == SortOption.PriceAscending && slot1.item.buyPrice > slot2.item.buyPrice ||
            sortOption == SortOption.Price && slot1.item.buyPrice < slot2.item.buyPrice ||
            //sortOption == SortOption.ItemTypeAsc && slot1.item.GetHashCode() > slot2.item.GetHashCode() ||
            sortOption == SortOption.Type && slot1.item.GetHashCode() < slot2.item.GetHashCode()

#if _iMMOITEMRARITY
            //|| sortOption == SortOption.RarityAscending && slot1.item.data.rarity > slot2.item.data.rarity 
            || sortOption == SortOption.Rarity && slot1.item.data.rarity < slot2.item.data.rarity
#endif
            )
        {
            return true;
        }
        return false;
    }

#endif
}
public enum SortOption
{
    //None,
    //ItemTypeAsc,
    Type,
    //PriceAscending,
    Price,
#if _iMMOITEMRARITY
    //RarityAscending,
    Rarity
#endif
}

