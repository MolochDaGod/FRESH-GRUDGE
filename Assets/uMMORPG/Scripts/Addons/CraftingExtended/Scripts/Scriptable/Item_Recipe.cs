using System.Text;
using UnityEngine;

#if _iMMOCRAFTING

// RECIPE ITEM

[CreateAssetMenu(fileName = "New Recipe", menuName = "ADDON/Crafting/RecipeItem", order = 999)]
public class Item_Recipe : UsableItem
{
    [Header("[-=-[ Recipe Item ]-=-]")]
    public Tmpl_Recipe[] learnedRecipes;

    public bool hasUnlimitedUse;

    public string tooltipHeader = "Learned Recipes:";

    // -----------------------------------------------------------------------------------
    // Use
    // -----------------------------------------------------------------------------------
    public override void Use(Player player, int inventoryIndex)
    {
        ItemSlot slot = player.inventory.slots[inventoryIndex];

        if (player.playerCraftingExtended.Crafting_LearnRecipe(learnedRecipes))
        {
            // always call base function too
            base.Use(player, inventoryIndex);

            //decrease amount on use if has no unlimited amount
            if (hasUnlimitedUse == false)
            {
                slot.DecreaseAmount(1);
                player.inventory.slots[inventoryIndex] = slot;
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // ToolTip
    // -----------------------------------------------------------------------------------
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());

        string recipeNames = tooltipHeader;

        foreach (Tmpl_Recipe recipe in learnedRecipes)
            recipeNames += "* " + recipe.name + "\n";

        tip.Replace("{SIMPLERECIPE}", recipeNames);

        return tip.ToString();
    }
    // -----------------------------------------------------------------------------------
}
#endif