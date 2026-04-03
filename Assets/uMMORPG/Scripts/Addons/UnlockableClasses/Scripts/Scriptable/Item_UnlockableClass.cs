using UnityEngine;

// RECIPE ITEM
[CreateAssetMenu(fileName = "Unlockable class item", menuName = "ADDON/Item/UnlockableClass Item", order = 999)]
public class Item_UnlockableClass : UsableItem
{
    [Header("[-=-[ Unlockable Class Item ]-=-]")]
    public Player playerClass;

    [Tooltip("Decrease amount by how many each use (can be 0)?")]
    public int decreaseAmount = 1;

    public string unlockPopupMessage = "New class unlocked: ";
    public byte unlockPopupIconID = 0;
    public byte unlockPopupSoundID = 0;
#if _SERVER
    // -----------------------------------------------------------------------------------
    // Use
    // -----------------------------------------------------------------------------------
    public override void Use(Player player, int inventoryIndex)
    {
#if _iMMOUNLOCKABLECLASSES
        ItemSlot slot = player.inventory.slots[inventoryIndex];

        // -- Only activate if enough charges left and not unlocked already
        if ((decreaseAmount == 0 || slot.amount >= decreaseAmount) && !player.playerAddonsConfigurator.HasUnlockedClass(playerClass))
        {
            // always call base function too
            base.Use(player, inventoryIndex);

            // -- Decrease Amount
            if (decreaseAmount != 0)
            {
                slot.DecreaseAmount(decreaseAmount);
                player.inventory.slots[inventoryIndex] = slot;
            }

            // -- Unlock the Class
            player.playerAddonsConfigurator.UnlockClass(playerClass, unlockPopupMessage);
        }
#endif
    }

#endif
    // -----------------------------------------------------------------------------------
}
