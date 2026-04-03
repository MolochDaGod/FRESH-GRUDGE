using UnityEngine;

// EQUIPMENT ITEM

public partial class EquipmentItem
{
    [Header("[-=-=-[ EQUIPABLE BAG ]-=-=-]")]
    public int extraInventorySize;

    public string bagCannotUnequipMsg = "You carry too many items - bag cannot be unequipped!";

    // -----------------------------------------------------------------------------------
    // canUnequipBag
    // -----------------------------------------------------------------------------------
    public bool canUnequipBag(Player player)
    {
        if (extraInventorySize == 0) return true;

        bool bValid = player.Tools_inventorySlotCount(player) < player.playerEquipableBag.inventorySize - extraInventorySize; // has to be less, because we have to take the unequipped item into account as well

        if (bValid == false)
            player.Tools_TargetAddMessage(bagCannotUnequipMsg);

        return bValid;
    }

    // -----------------------------------------------------------------------------------
    // CanUnequip (Swapping)
    // -----------------------------------------------------------------------------------
    private void CanUnequip_EquipableBag(Player player, int inventoryIndex, int equipmentIndex, MutableWrapper<bool> bValid)
    {
        if (!bValid.Value) return; //when not valid, we dont have to check at all

        bValid.Value = player.Tools_inventorySlotCount(player) < player.playerEquipableBag.inventorySize - extraInventorySize; // has to be less, because we have to take the unequipped item into account as well

        if (bValid.Value == false)
            player.Tools_TargetAddMessage(bagCannotUnequipMsg);
    }

    // -----------------------------------------------------------------------------------
    // CanUnequipClick (Clicking)
    // -----------------------------------------------------------------------------------
    private void CanUnequipClick_EquipableBag(Player player, EquipmentItem item, MutableWrapper<bool> bValid)
    {
        if (!bValid.Value) return; //when not valid, we dont have to check at all

        bValid.Value = player.Tools_inventorySlotCount(player) < player.playerEquipableBag.inventorySize - extraInventorySize; // has to be less, because we have to take the unequipped item into account as well

        if (bValid.Value == false)
            player.Tools_TargetAddMessage(bagCannotUnequipMsg);
    }

    // -----------------------------------------------------------------------------------
}
