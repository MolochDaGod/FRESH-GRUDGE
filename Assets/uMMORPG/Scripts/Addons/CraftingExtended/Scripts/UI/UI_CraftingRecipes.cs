using UnityEngine;
#if _iMMOCRAFTING
// ===================================================================================
// CRAFTING UI
// ===================================================================================
public partial class UI_CraftingRecipes : MonoBehaviour
{
    public GameObject panel;
    public Transform content;
    public UI_RecipeSlot slotPrefab;
    public KeyCode hotKey = KeyCode.C;

    // -----------------------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------------------
    private void Update()
    {
        Player player = Player.localPlayer;
        if (!player) return;

        // hotkey (not while typing in chat, etc.)
        if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
            panel.SetActive(!panel.activeSelf);

        if (panel.activeSelf && player.playerCraftingExtended._recipes.Count > 0)
        {
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.playerCraftingExtended._recipes.Count, content);

            for (int i = 0; i < content.childCount; i++)
            {
                content.GetChild(i).GetComponent<UI_RecipeSlot>().Show(player.playerCraftingExtended._recipes[i]);
            }
        }
    }
}
#endif