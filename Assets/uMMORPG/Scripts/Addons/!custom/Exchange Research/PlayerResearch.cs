using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerResearch : NetworkBehaviour
{
    [Header("Components")]
    public Player player;
    void OnDragAndDrop_InventorySlot_ResearchSlot(int[] slotIndices)
    {
        ResearchPanel.instance.itemIndex = slotIndices[0];
    }
    [Command]
    public void CmdResearchItem(int itemIndex)
    {
        // should only be called while CRAFTING and if recipe still valid
        // (no one should touch 'craftingRecipe', but let's just be sure.
        // -> we already validated everything in CmdCraft. let's just craft.
        if (itemIndex != -1)
        {
            // enough space?
            Item result = new Item(player.inventory.slots[itemIndex].item.data.researchResult);
            if (player.inventory.CanAdd(result, 1))
            {
                bool hadAllItems = true;
                // remove the ingredients from inventory in any case
                foreach (Tools_ItemRequirement ingredient in player.inventory.slots[itemIndex].item.data.researchCosts.requiredItems)
                {
                    int ingrIndex = player.inventory.GetItemIndexByName(ingredient.item.name);
                    if (ingredient.amount > 0 && ingredient.item != null && ingrIndex != -1 && player.inventory.slots[ingrIndex].amount >= ingredient.amount)
                    {
                        player.inventory.Remove(new Item(ingredient.item), ingredient.amount);
                    }
                    else
                    {
                        hadAllItems = false;
                        break;
                    }
                }
                if (hadAllItems)
                {
                    player.gold -= player.inventory.slots[itemIndex].item.data.researchCosts.gold;
                    //remove the item itself
                    player.inventory.Remove(player.inventory.slots[itemIndex].item, 1);
                    // add result item to inventory
                    player.inventory.Add(result, 1);
                }

            }
        }
    }
}
