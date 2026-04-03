using UnityEngine;

// PLAYER WAREHOUSE - ITEM
[CreateAssetMenu(menuName = "ADDON/Item/Item Warehouse", order = 998)]
public class Tmpl_ItemPlayerWarehouse : UsableItem
{
    [Header("[-=-[ Warehouse Item ]-=-]")]
    [Tooltip("Decrease amount by how many each use (can be 0)?")]
    public int decreaseAmount = 1;

    // -----------------------------------------------------------------------------------
    // Use
    // @Server
    // -----------------------------------------------------------------------------------
    public override void Use(Player player, int inventoryIndex)
    {
        ItemSlot slot = player.inventory.slots[inventoryIndex];

        // -- Only activate if enough charges left
        if (decreaseAmount == 0 || slot.amount >= decreaseAmount)
        {
            // always call base function too
            base.Use(player, inventoryIndex);

            // -- Decrease Amount
            if (decreaseAmount != 0)
            {
                slot.DecreaseAmount(decreaseAmount);
                player.inventory.slots[inventoryIndex] = slot;
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // OnUsed
    // @Client
    // -----------------------------------------------------------------------------------
    public override void OnUsed(Player player)
    {
        player.playerAddonsConfigurator.warehouseConfiguration.uiEventPlayerWarehouse.TriggerEvent();
    }

    // -----------------------------------------------------------------------------------
}