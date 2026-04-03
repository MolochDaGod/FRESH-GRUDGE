using System.Linq;
using UnityEngine;

// EQUIPMENT ITEM
public partial class EquipmentItem
{
    [Header("[-=-=-[ CURSED EQUIPMENT ]-=-=-]")]
    [Tooltip("Set to >0 to make this Item cursed. Cursed items cannot be unequipped, once equipped.")]
    [Range(0, 9)] public int cursedLevel;

    [Tooltip("Set to >0 to override cursed Items. This item can swap its equipment slot with a cursed item of lesser curse level.")]
    [Range(0, 9)] public int overrideCursedLevel;

    [Tooltip("Check to nullify all curses on all other equipped item, while this item is equipped.")]
    public bool nullsAllCurses;

    public string cursedCannotUnequipMsg = "This item is cursed and cannot be unequipped!";

    // -----------------------------------------------------------------------------------
    // CanUnequip (Swapping)
    // -----------------------------------------------------------------------------------
    private void CanUnequip_CursedEquipment(Player player, int inventoryIndex, int equipmentIndex, MutableWrapper<bool> bValid)
    {
        if (!bValid.Value) return; //when not valid, we dont have to check at all

        if (player.equipment.slots.Any(x => (x.amount > 0 && ((EquipmentItem)x.item.data).nullsAllCurses == true)))
        { // no need to check if any item can nullify curses
            bValid.Value = true;
            return;
        }
        int overrideLevel = 0;

        if (player.inventory.slots[inventoryIndex].amount > 0)
        {
            EquipmentItem item = (EquipmentItem)player.inventory.slots[inventoryIndex].item.data;
            overrideLevel = item.overrideCursedLevel;
        }

        bValid.Value = CanEquip(player, inventoryIndex, equipmentIndex);//&&
//                (player.equipment.slots.Any(x => (x.amount > 0 && (cursedLevel <= 0 || (cursedLevel > 0 && overrideLevel >= cursedLevel)))));
        if (bValid.Value == false)
            player.Tools_TargetAddMessage(cursedCannotUnequipMsg);
    }

    // -----------------------------------------------------------------------------------
    // CanUnequipClick (Clicking)
    // -----------------------------------------------------------------------------------
    private void CanUnequipClick_CursedEquipment(Player player, EquipmentItem item, MutableWrapper<bool> bValid)
    {
        if (!bValid.Value) return; //when not valid, we dont have to check at all

        if (player.equipment.slots.Any(x => (x.amount > 0 && ((EquipmentItem)x.item.data).nullsAllCurses == true)))
        { // no need to check if any item can nullify curses
            bValid.Value = true;
            return;
        }

        int overrideLevel = item.overrideCursedLevel;

        bValid.Value = player.equipment.slots.Any(x => (x.amount > 0 && (cursedLevel <= 0 || (cursedLevel > 0 && overrideLevel >= cursedLevel))));

        if (bValid.Value == false)
            player.Tools_TargetAddMessage(cursedCannotUnequipMsg);
    }

    // -----------------------------------------------------------------------------------
}
