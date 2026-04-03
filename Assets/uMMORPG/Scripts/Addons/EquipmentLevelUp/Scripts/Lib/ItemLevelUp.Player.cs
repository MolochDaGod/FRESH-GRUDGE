using Mirror;
using UnityEngine;

public partial class Player 
{

    [Command]
    public void Cmd_updrageEquipmentItem(int inventoryIndex)
    {
#if _SERVER
        ItemSlot slot = inventory.slots[inventoryIndex];
        if (slot.amount > 0 && slot.item.data is EquipmentItem equipableItem)
        {
            // verify the equipment status
            if (equipableItem.enableLevelUp && slot.item.equipmentLevel < equipableItem.LevelUpParameters.Length)
            {
                if (equipableItem.LevelUpParameters[slot.item.equipmentLevel].levelUpCost.CheckCost(this))
                {
                    equipableItem.LevelUpParameters[slot.item.equipmentLevel].levelUpCost.PayCost(this);

                    // Attend j'ai déjà à faire plusieurs calculs 
                    bool destroy = (Random.Range(0, 100) < equipableItem.LevelUpParameters[slot.item.equipmentLevel].destroyRate);
                    bool succesUpdate = (Random.Range(0, 100) < equipableItem.LevelUpParameters[slot.item.equipmentLevel].successRate);

                    if (destroy && equipableItem.LevelUpParameters[slot.item.equipmentLevel].enableDestroyChance)
                    {
                        slot.amount = 0;
                        inventory.slots[inventoryIndex] = slot;
                        UpdateEquipmentLevelUpEvent();
                        Tools_TargetAddMessage("Sorry your Item is destroyed !");
                    }
                    else
                    {
                        if (succesUpdate)
                        {
                            int newLevel = slot.item.equipmentLevel + 1;
                            slot.item.equipmentLevel = newLevel;
                            inventory.slots[inventoryIndex] = slot;
                            UpdateEquipmentLevelUpEvent();
                        }
                        else
                        {
                            Tools_TargetAddMessage("Sorry fail updage !");
                            UpdateEquipmentLevelUpEvent();
                        }
                    }
                }
            }
        }
#endif
    }

    [ClientRpc]

    private void UpdateEquipmentLevelUpEvent()
    {
#if _CLIENT
        inventory.EquipmentlevelUpEvent.TriggerEvent();
#endif
    }
}
