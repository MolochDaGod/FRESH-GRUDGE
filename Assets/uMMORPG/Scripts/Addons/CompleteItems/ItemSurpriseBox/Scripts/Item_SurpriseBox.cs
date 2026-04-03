using UnityEngine;

// SURPRISE BOX ITEM

[CreateAssetMenu(menuName = "ADDON/Item/Item SurpriseBox", order = 999)]
public class Item_SurpriseBox : UsableItem
{
    [Header("[-=-[ Surprise Box Item ]-=-]")]
    public Tools_InteractionRewards rewards;

    [Tooltip("Decrease amount by how many each use (can be 0)?")]
    public int decreaseAmount = 1;

    [Tooltip("Message shown if there is not enough free space in inventory.")]
    public string failMessage = "You cannot use that right now!";

#if _SERVER
    // -----------------------------------------------------------------------------------
    // Use
    // -----------------------------------------------------------------------------------
    public override void Use(Player player, int inventoryIndex)
    {
        ItemSlot slot = player.inventory.slots[inventoryIndex];

        // -- Only activate if enough charges left and can fit all items
        if (decreaseAmount == 0 || slot.amount >= decreaseAmount)
        {
            if (player.isAlive && player.inventory.SlotsFree() >= rewards.items.Length)
            {
                // always call base function too
                base.Use(player, inventoryIndex);

                // -- activate surprise box
                rewards.gainRewards(player);

                // decrease amount
                slot.DecreaseAmount(decreaseAmount);
                player.inventory.slots[inventoryIndex] = slot;
            }
            else
            {
                player.Tools_TargetAddMessage(failMessage);
            }
        }
    }
#endif
    // -----------------------------------------------------------------------------------
}
