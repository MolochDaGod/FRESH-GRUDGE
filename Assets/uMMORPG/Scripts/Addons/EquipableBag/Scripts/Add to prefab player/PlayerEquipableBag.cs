using Mirror;
using UnityEngine;

// PLAYER
public class PlayerEquipableBag : NetworkBehaviour
{

    public Player player;
    public PlayerEquipment playerEquipment;

    [Header("[-=-[ INVENTORY SIZE ]-=-]")]
    public int _inventorySize = 30;

    protected int inventoryExtraSize;

    // -----------------------------------------------------------------------------------
    // inventorySize
    // -----------------------------------------------------------------------------------
    public int inventorySize
    {
        get
        {
            calculateInventoryExtraSlots();
            return _inventorySize + inventoryExtraSize;
        }
    }

    // -----------------------------------------------------------------------------------
    // OnStartLocalPlayer
    // -----------------------------------------------------------------------------------
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        playerEquipment.onEquipmentChanged.AddListener(OnEquipmentChanged_EquipableBag);
        OnEquipmentChanged_EquipableBag();
    }

    // -----------------------------------------------------------------------------------
    // OnEquipmentChanged_EquipableBag
    // @Client
    // -----------------------------------------------------------------------------------
    private void OnEquipmentChanged_EquipableBag()
    {
        calculateInventoryExtraSlots();

        if (inventorySize != player.inventory.slots.Count)
            Cmd_calculcateInvenvtory();
    }

    // -----------------------------------------------------------------------------------
    // Cmd_calculcateInvenvtory
    // @Client -> @Server
    // -----------------------------------------------------------------------------------
    [Command]
    private void Cmd_calculcateInvenvtory()
    {
#if _SERVER
        calculateInventoryExtraSlots();

        if (inventorySize > player.inventory.slots.Count)
        {
            for (int i = player.inventory.slots.Count; i < inventorySize; i++)
                player.inventory.slots.Add(new ItemSlot());
        }
        else if (inventorySize < player.inventory.slots.Count)
        {
            for (int i = player.inventory.slots.Count - 1; player.inventory.slots.Count > inventorySize && i >= 0; i--)
            {
                if (player.inventory.slots[i].amount == 0)
                    player.inventory.slots.RemoveAt(i);
            }
        }
#endif
    }

    // -----------------------------------------------------------------------------------
    // calculateInventoryExtraSlots
    // -----------------------------------------------------------------------------------
    public void calculateInventoryExtraSlots()
    {
        inventoryExtraSize = 0;

        for (int i = 0; i < player.equipment.slots.Count; ++i)
        {
            if (player.equipment.slots[i].amount > 0)
                inventoryExtraSize += ((EquipmentItem)player.equipment.slots[i].item.data).extraInventorySize;
        }
    }

    // -----------------------------------------------------------------------------------
}
