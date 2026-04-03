using System.Text;
using UnityEngine;

// ACCOUNT UNLOCKABLE ITEM
[CreateAssetMenu(menuName = "ADDON/Account Unlockable/Item Unlockable", order = 999)]
public class Item_AccountUnlockable : UsableItem
{
    //[Header("[-=-[ Account Unlockable Item ]-=-]")]
    //public Item_Unlock unlockableName;

    [Header("[-=-[ Message Tooltips ]-=-]")]
    public string alreadyUnlocked = "<color=red><b>no</b> (already unlocked)</color>";
    public string unlockable = "<color=green><b>yes</b> (not unlocked)</color>";

    [Tooltip("Decrease amount by how many each use (can be 0)?")]
    public int decreaseAmount = 1;

    public string unlockPopupMessage = "You unlocked: ";
    public byte unlockPopupIconID = 0;
    public byte unlockPopupSoundID = 0;


    // -----------------------------------------------------------------------------------
    // CanUse
    // Server & Client
    // -----------------------------------------------------------------------------------
    public override bool CanUse(Player player, int inventoryIndex)
    {
        return !player.playerAddonsConfigurator.HasAccountUnlock(name);
    }

#if _SERVER
    // -----------------------------------------------------------------------------------
    // Use
    // Server
    // -----------------------------------------------------------------------------------
    public override void Use(Player player, int inventoryIndex)
    {
        ItemSlot slot = player.inventory.slots[inventoryIndex];

        // -- Only activate if enough charges left and not unlocked already
        if ((decreaseAmount == 0 || slot.amount >= decreaseAmount) && !player.playerAddonsConfigurator.HasAccountUnlock(name))
        {
            // always call base function too
            base.Use(player, inventoryIndex);

            // -- Decrease Amount
            if (decreaseAmount != 0)
            {
                slot.DecreaseAmount(decreaseAmount);
                player.inventory.slots[inventoryIndex] = slot;
            }

            // -- Unlock the Unlockable
            player.playerAddonsConfigurator.AccountUnlock(name, unlockPopupMessage);
        }
    }
#endif

    // -----------------------------------------------------------------------------------
    // ToolTip
    // Client
    // -----------------------------------------------------------------------------------
    public override string ToolTip()
    {
        Player player = Player.localPlayer;
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{USABLE}", (player.playerAddonsConfigurator.HasAccountUnlock(name)) ? alreadyUnlocked : unlockable);
        tip.Replace("{UNLOCKNAME}", name);
        return tip.ToString();
    }
    // -----------------------------------------------------------------------------------
}