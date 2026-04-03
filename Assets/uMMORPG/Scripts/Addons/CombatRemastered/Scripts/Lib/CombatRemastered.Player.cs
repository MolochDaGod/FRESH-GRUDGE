using Mirror;
using UnityEngine;

public partial class Player
{
    [Header("[Component : player Combat Remastered]")]
    public PlayerCombatRemastered combatRemastered;

    [HideInInspector] public int autoAttack = 0;

    // Checks if we have the required equipment to cast that skill.
    public bool CheckEquipSkills(ScriptableSkill skill)
    {
        for (int i = 0; i < equipment.slots.Count; i++)
        {
            if (equipment.slots[i].amount > 0)
            {
                //EquipmentItem equipItem = equipment.slots[i].item.data as EquipmentItem;
                WeaponItem equipItem = (WeaponItem)equipment.slots[i].item.data;

                if ((int)equipItem.weaponType == (int)skill.skillType || skill.skillType == WeaponType.Unarmed)
                {
                    return true;
                }
            }
        }

        return false;
    }

    // Removes unwanted equipment when switching to different weapon types.
    [Command]
    public void CmdAutoRemoveEquipment(int index)
    {
        // validate
        if (0 <= index && index < equipment.slots.Count && equipment.slots[index].amount > 0)
        {
            // check inventory for free slot and pass it to swapinventoryequip()
            ItemSlot item = equipment.slots[index];

            if (inventory.SlotsFree() >= item.amount)
            {
                if (item.amount > 0)
                {
                    inventory.Add(item.item, 1);
                    item.DecreaseAmount(1);
                    equipment.slots[index] = item;
                }
            }
        }
    }
}
