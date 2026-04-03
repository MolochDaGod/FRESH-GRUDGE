using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_QuickCraft : MonoBehaviour
{
	public GameObject panel;
	public UI_CraftingUnlearn unlearnPanel;
	public Slot_Crafting slotPrefab;
	public Transform content;
	public UI_CraftingButton[] categoryButtons;
	public string categoryAll = "All";

	private string currentCategory;
	private List<Tmpl_Recipe> recipes;
	private CraftingProfessionTemplate profession;


    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    private void Update()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        // Only refresh the UI when the panel is active  
        if (panel.activeSelf)
        {
            RefreshCraftingSlots(player);
        }
        else
        {
            currentCategory = "";
        }
    }

    private void RefreshCraftingSlots(Player player)
    {
        recipes = new List<Tmpl_Recipe>();
        for (int i = 0; i < player.playerCraftingExtended.playerSpecificRecipes.Count; ++i)
        {
            recipes.Add(player.playerCraftingExtended.playerSpecificRecipes[i]);
        }

        int rcount = getRecipeCount();
        int t_index = -1;
        UIUtils.BalancePrefabs(slotPrefab.gameObject, rcount, content);

        for (int i = 0; i < recipes.Count; ++i)
        {
            if (canCraft(i))
            {
                t_index++;
                Tmpl_Recipe recipe = recipes[i];
                Slot_Crafting slot = content.GetChild(t_index).GetComponent<Slot_Crafting>();

                slot.Show(recipe, player.playerCraftingExtended.Crafting_CanBoost(recipe, 1));
                bool canCraft = player.playerCraftingExtended.Crafting_CraftValidation(recipe, 1, slot.boost) && !player.playerCraftingExtended.Crafting_isBusy();
                slot.actionButton.interactable = canCraft;

                slot.unlearnButton.interactable = true;
                slot.unlearnButton.onClick.SetListener(() =>
                {
                    unlearnPanel.Show(recipe);
                });

                slot.actionButton.onClick.SetListener(() =>
                {
                    if (canCraft) // Ensure crafting is only triggered if it's allowed  
                    {
                        player.playerCraftingExtended.Crafting_startCrafting(recipe, 1, slot.boost, true);
                        panel.SetActive(false);
                    }
                });

                slot.overlay.gameObject.SetActive(false);
            }
            else
            {
                t_index++;
                Tmpl_Recipe recipe = recipes[i];
                Slot_Crafting slot = content.GetChild(t_index).GetComponent<Slot_Crafting>();
                slot.Show(recipe, player.playerCraftingExtended.Crafting_CanBoost(recipe, 1));
                slot.actionButton.interactable = false;
                slot.overlay.gameObject.SetActive(true);

                slot.unlearnButton.interactable = true;
                slot.unlearnButton.onClick.SetListener(() =>
                {
                    unlearnPanel.Show(recipe);
                });
            }
        }
    }

    // -----------------------------------------------------------------------------------
    // changeCategory
    // -----------------------------------------------------------------------------------
    public void changeCategory(string newCategory)
	{
		currentCategory = newCategory;

		for (int i = 0; i < content.childCount; ++i)
		{
			Destroy(content.GetChild(i).gameObject);
		}

		Invoke("Refresh", .05f);


	}

	// -----------------------------------------------------------------------------------
	// getRecipeCount
	// -----------------------------------------------------------------------------------
	private int getRecipeCount()
	{
		return recipes.Count;
	}

	// -----------------------------------------------------------------------------------
	// canCraft
	// -----------------------------------------------------------------------------------
	private bool canCraft(int index)
	{
		Player player = Player.localPlayer;
		if (!player) return false;

		//((recipes[index].category == currentCategory || currentCategory == categoryAll) &&
		return player.playerCraftingExtended.Crafting_CraftValidation(recipes[index], 1, false);
	}
}
