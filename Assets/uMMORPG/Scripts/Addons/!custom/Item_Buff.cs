using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror; // Add this line to include the Mirror namespace

// ITEM BUFF
[CreateAssetMenu(menuName = "ADDON/Item/Item Buff", order = 999)]
public class Item_Buff : UsableItem
{
    [Header("[-=-=-[ Buff Item ]-=-=-]")]
    public BuffSkill applyBuff;
    public int buffLevel;
    [Range(0, 1)]
    public float buffChance;
    public float distance;
    public string successMessage = "You buffed yourself with {1}";

    [Tooltip("Decrease amount by how many each use (can be 0)?")]
    public int decreaseAmount = 1;

    // -----------------------------------------------------------------------------------
    // Use
    // -----------------------------------------------------------------------------------
    public override void Use(Player player, int inventoryIndex)
    {
        ItemSlot slot = player.inventory.slots[inventoryIndex];

        // -- Only activate if enough charges left
        if (decreaseAmount == 0 || slot.amount >= decreaseAmount)
        {
            // always call base function too
            base.Use(player, inventoryIndex);

            // Always apply the buff to the player
            player.Tools_ApplyBuff(applyBuff, buffLevel, buffChance);

            player.Tools_TargetAddMessage(string.Format(successMessage, player.name, applyBuff.name));

            // decrease amount
            slot.DecreaseAmount(decreaseAmount);
            player.inventory.slots[inventoryIndex] = slot;
        }
    }

    // -----------------------------------------------------------------------------------
}
// Ensure that there is only one definition of the Load_Example method in the CharactersAvailableMsg class
public partial class CharactersAvailableMsg : NetworkMessage
{
    // Existing Load_Example method
    public void Load_Example(NetworkReader reader)
    {
        // Method implementation
    }

    // Remove or rename any duplicate Load_Example methods
    // public void Load_Example(NetworkReader reader)
    // {
    //     // Duplicate method implementation
    // }
}