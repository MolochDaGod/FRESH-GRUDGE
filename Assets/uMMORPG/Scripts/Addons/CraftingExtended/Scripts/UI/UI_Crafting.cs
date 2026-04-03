using System.Collections.Generic;
using UnityEngine;
#if _iMMOCRAFTING

// CRAFTING
public partial class UI_Crafting : MonoBehaviour
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

    protected GameObject instance;

    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    private void Update()
    {
        Refresh();
    }

    // -----------------------------------------------------------------------------------
    // Refresh
    // -----------------------------------------------------------------------------------
    public void Refresh()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        if (panel.activeSelf && instance != null && UMMO_Tools.Tools_CheckSelectionHandling(instance))
        {
            int rcount = GetRecipeCount();
            int t_index = -1;

            UIUtils.BalancePrefabs(slotPrefab.gameObject, rcount, content);

            for (int i = 0; i < recipes.Count; ++i)
            {
                if (recipes[i].category == currentCategory || currentCategory == categoryAll)
                {
                    t_index++;
                    Tmpl_Recipe recipe = recipes[i];

                    Slot_Crafting slot = content.GetChild(t_index).GetComponent<Slot_Crafting>();
                    slot.Show(recipe, player.playerCraftingExtended.Crafting_CanBoost(recipe, slot.amount));

                    // Here you check if it can be crafted
                    bool canCraft = player.playerCraftingExtended.Crafting_CraftValidation(recipe, slot.amount, slot.boost) && !player.playerCraftingExtended.Crafting_isBusy();
                    slot.actionButton.interactable = canCraft;

                    slot.unlearnButton.interactable = true;
                    slot.unlearnButton.onClick.SetListener(() => {
                        unlearnPanel.Show(recipe);
                    });

                    slot.actionButton.onClick.SetListener(() => {
                        if (canCraft) // Ensure crafting is only triggered if it's allowed  
                        {
                            player.playerCraftingExtended.Crafting_startCrafting(recipe, slot.amount, slot.boost, false);
                            panel.SetActive(false);
                        }
                    });
                    slot.overlay.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            currentCategory = "";
            panel.SetActive(false); // hide
        }
    }

    // -----------------------------------------------------------------------------------
    // Show
    // -----------------------------------------------------------------------------------
    public void Show(GameObject _instance, CraftingProfessionTemplate p, List<Tmpl_Recipe> r)
    {
        Player player = Player.localPlayer;
        if (!player) return;

        instance = _instance;
        profession = p;
        for (int i = 0; i < categoryButtons.Length; ++i)
        {
            if (profession.categories.Length-1 >= i)
            {
                categoryButtons[i].gameObject.SetActive(true);
                categoryButtons[i].SetCategory(profession.categories[i]);
            }
            else
            {
                categoryButtons[i].gameObject.SetActive(false);
            }
        }

        recipes = new List<Tmpl_Recipe>();
        recipes.Clear();
        recipes = r;

        currentCategory = categoryAll;

        ChangeCategory(currentCategory);
    }

    // -----------------------------------------------------------------------------------
    // changeCategory
    // -----------------------------------------------------------------------------------
    public void ChangeCategory(string newCategory)
    {
        currentCategory = newCategory;

        for (int i = 0; i < content.childCount; ++i)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        Invoke(nameof(Refresh), .05f);

        panel.SetActive(true);
    }

    // -----------------------------------------------------------------------------------
    // getRecipeCount
    // -----------------------------------------------------------------------------------
    private int GetRecipeCount()
    {
        // Return total count of recipes that belong to the current category
        int count = 0;
        for (int i = 0; i < recipes.Count; ++i)
        {
            if (recipes[i].category == currentCategory || currentCategory == categoryAll)
                count++;
        }
        return count;
    }

    // -----------------------------------------------------------------------------------
    // canCraft
    // -----------------------------------------------------------------------------------
    private bool CanCraft(int index)
    {
        Player player = Player.localPlayer;
        if (!player) return false;

        return ((recipes[index].category == currentCategory || currentCategory == categoryAll) && player.playerCraftingExtended.Crafting_CraftValidation(recipes[index], 1, false));
    }

    // -----------------------------------------------------------------------------------
}
#endif