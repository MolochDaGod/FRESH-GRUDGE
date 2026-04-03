using UnityEngine;
#if _iMMOPRESTIGECLASSES
// ACQUIRE PRESTIGE CLASS TEMPLATE
[CreateAssetMenu(menuName = "ADDON/Item/Acquire Prestige Class Item", order = 999)]
public class AcquirePrestigeClassItemTemplate : UsableItem
{
    [Header("Usage")]
    public PrestigeClassTemplate prestigeClass;

    public string successMessage = "You acquired the prestige class: ";

    [Tooltip("Decrease amount by how many each use (can be 0)?")]
    public int decreaseAmount = 1;

    // -----------------------------------------------------------------------------------
    // CanUse
    // -----------------------------------------------------------------------------------
    public override bool CanUse(Player player, int inventoryIndex)
    {
        return minLevel < player.level.current;
    }

    // -----------------------------------------------------------------------------------
    // Use
    // -----------------------------------------------------------------------------------

#if _SERVER
    public override void Use(Player player, int inventoryIndex)
    {
        ItemSlot slot = player.inventory.slots[inventoryIndex];

        // -- Only activate if enough charges left
        if (decreaseAmount == 0 || slot.amount >= decreaseAmount)
        {
            // always call base function too
            base.Use(player, inventoryIndex);

            player.playerAddonsConfigurator.prestigeClass = prestigeClass;
            player.Tools_TargetAddMessage(successMessage + prestigeClass.name);

            // decrease amount
            slot.DecreaseAmount(decreaseAmount);
            player.inventory.slots[inventoryIndex] = slot;
        }
    }
#endif

    // -----------------------------------------------------------------------------------
}
#endif