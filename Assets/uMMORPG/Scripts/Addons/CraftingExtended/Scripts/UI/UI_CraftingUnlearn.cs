using UnityEngine;
using UnityEngine.UI;
#if _iMMOCRAFTING
// UI CRAFTING UNLEARN
public partial class UI_CraftingUnlearn : MonoBehaviour
{
    public GameObject panel;
    public GameObject parentPanel;
    public Text text;

    public string unlearnText = "Do you want to unlearn: ";

    [HideInInspector] public Tmpl_Recipe recipe;

    // -----------------------------------------------------------------------------------
    // Show
    // -----------------------------------------------------------------------------------
    public void Show(Tmpl_Recipe newRecipe)
    {
        recipe = newRecipe;
        text.text = unlearnText + recipe.name;
        panel.SetActive(true);
    }

    // -----------------------------------------------------------------------------------
    // onClickUnlearn
    // -----------------------------------------------------------------------------------
    public void onClickUnlearn()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        player.playerCraftingExtended.Cmd_Crafting_unlearnRecipe(recipe.name);

        panel.SetActive(false);
        parentPanel.SetActive(false);
    }

    // -----------------------------------------------------------------------------------
    // onClickCancel
    // -----------------------------------------------------------------------------------
    public void onClickCancel()
    {
        panel.SetActive(false);
    }
    // -----------------------------------------------------------------------------------
}
#endif